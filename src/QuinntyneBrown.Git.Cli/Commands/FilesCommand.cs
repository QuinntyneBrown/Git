using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuinntyneBrown.Git.Core;
using QuinntyneBrown.Git.Core.Services;

namespace QuinntyneBrown.Git.Cli.Commands;

public static class FilesCommand
{
    public static Command Create(IServiceProvider services)
    {
        var urlArgument = new Argument<string>("url", "The URL of the git repository");
        var branchOption = new Option<string?>("--branch", "The branch to clone or verify");
        branchOption.AddAlias("-b");
        var outputOption = new Option<string?>("--output", "File path to write the output to. Defaults to <repo-name>.txt in the current directory.");
        outputOption.AddAlias("-o");

        var command = new Command("files", "Aggregate all repository files into a single text file optimized for LLM consumption")
        {
            urlArgument,
            branchOption,
            outputOption
        };

        command.SetHandler(async (string url, string? branch, string? output) =>
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Files");
            var gitService = services.GetRequiredService<IGitService>();
            var tempFolderService = services.GetRequiredService<ITempFolderService>();

            // Clone or reuse existing
            if (tempFolderService.FolderExists(url))
            {
                var existingPath = tempFolderService.GetFolderPath(url);

                if (!string.IsNullOrWhiteSpace(branch))
                {
                    var currentBranch = await gitService.GetCurrentBranchAsync(existingPath);
                    if (!string.Equals(currentBranch, branch, StringComparison.OrdinalIgnoreCase))
                    {
                        logger.LogInformation("Existing clone is on a different branch. Re-cloning.");
                        tempFolderService.DeleteFolder(url);
                    }
                }
            }

            if (!tempFolderService.FolderExists(url))
            {
                tempFolderService.EnsureBasePath();
                var folder = tempFolderService.GetFolderPath(url);
                var success = await gitService.CloneAsync(url, folder, branch);

                if (!success)
                {
                    logger.LogError("Failed to clone repository");

                    if (Directory.Exists(folder))
                    {
                        Directory.Delete(folder, true);
                    }

                    return;
                }

                tempFolderService.WriteMarker(url);
            }

            var repoPath = tempFolderService.GetFolderPath(url);
            var content = await RepoContentAggregator.AggregateAsync(repoPath);

            // Resolve output path
            var outputPath = ResolveOutputPath(url, output);

            var outputDir = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrWhiteSpace(outputDir) && !Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            await File.WriteAllTextAsync(outputPath, content);
            Console.WriteLine(outputPath);
        }, urlArgument, branchOption, outputOption);

        return command;
    }

    private static string ResolveOutputPath(string url, string? output)
    {
        if (!string.IsNullOrWhiteSpace(output))
        {
            return Path.IsPathFullyQualified(output)
                ? output
                : Path.Combine(Directory.GetCurrentDirectory(), output);
        }

        // Derive a filename from the repo URL
        var repoName = url.TrimEnd('/').Split('/').LastOrDefault() ?? "repo";

        if (repoName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
        {
            repoName = repoName[..^4];
        }

        return Path.Combine(Directory.GetCurrentDirectory(), $"{repoName}.txt");
    }
}
