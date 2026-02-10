using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuinntyneBrown.Git.Core;
using QuinntyneBrown.Git.Core.Services;

namespace QuinntyneBrown.Git.Cli.Commands;

public static class DiffCommand
{
    public static Command Create(IServiceProvider services)
    {
        var urlArgument = new Argument<string>("url", "The full URL of the git repository (branch is inferred from the URL)");
        var defaultBranchOption = new Option<string>("--default-branch", () => "main", "The default branch to diff against");
        defaultBranchOption.AddAlias("-d");
        var outputOption = new Option<string?>("--output", "File path to write the diff to. Uses the current directory if only a filename is provided.");
        outputOption.AddAlias("-o");
        var includeFilesOption = new Option<string?>("--include-files", "Write an additional text file with the default branch contents optimized for LLM consumption. Provide a file path or filename.");

        var command = new Command("diff", "Show the diff between a branch inferred from the URL and the default branch")
        {
            urlArgument,
            defaultBranchOption,
            outputOption,
            includeFilesOption
        };

        command.SetHandler(async (string url, string defaultBranch, string? output, string? includeFiles) =>
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Diff");
            var gitService = services.GetRequiredService<IGitService>();
            var tempFolderService = services.GetRequiredService<ITempFolderService>();

            var (cloneUrl, branch) = GitUrlParser.Parse(url);

            if (string.IsNullOrWhiteSpace(branch))
            {
                logger.LogError("Could not infer a branch from the URL. Provide a URL containing a branch path (e.g. https://github.com/user/repo/tree/my-branch).");
                return;
            }

            if (string.Equals(branch, defaultBranch, StringComparison.OrdinalIgnoreCase))
            {
                logger.LogError("The inferred branch '{Branch}' is the same as the default branch.", branch);
                return;
            }

            // Ensure the repo is cloned
            if (!tempFolderService.FolderExists(cloneUrl))
            {
                tempFolderService.EnsureBasePath();
                var success = await gitService.CloneAsync(cloneUrl, tempFolderService.GetFolderPath(cloneUrl));

                if (!success)
                {
                    logger.LogError("Failed to clone repository");
                    return;
                }

                tempFolderService.WriteMarker(cloneUrl);
            }

            var repoPath = tempFolderService.GetFolderPath(cloneUrl);

            // Fetch latest so both branches are available
            await gitService.FetchAsync(repoPath);

            var diff = await gitService.DiffAsync(repoPath, $"origin/{defaultBranch}", $"origin/{branch}");

            if (diff == null)
            {
                logger.LogError("Failed to generate diff");
                return;
            }

            if (string.IsNullOrWhiteSpace(diff))
            {
                Console.WriteLine("No differences found.");
                return;
            }

            if (!string.IsNullOrWhiteSpace(output))
            {
                var outputPath = ResolvePath(output);
                EnsureDirectory(outputPath);
                await File.WriteAllTextAsync(outputPath, diff);
                Console.WriteLine(outputPath);
            }
            else
            {
                Console.WriteLine(diff);
            }

            if (!string.IsNullOrWhiteSpace(includeFiles))
            {
                // Checkout the default branch, aggregate files, then restore
                var checkoutSuccess = await gitService.CheckoutAsync(repoPath, $"origin/{defaultBranch}");

                if (!checkoutSuccess)
                {
                    logger.LogError("Failed to checkout default branch '{Branch}' for file aggregation.", defaultBranch);
                    return;
                }

                var content = await RepoContentAggregator.AggregateAsync(repoPath);
                var filesPath = ResolvePath(includeFiles);
                EnsureDirectory(filesPath);
                await File.WriteAllTextAsync(filesPath, content);
                Console.WriteLine(filesPath);
            }
        }, urlArgument, defaultBranchOption, outputOption, includeFilesOption);

        return command;
    }

    private static string ResolvePath(string path)
    {
        return Path.IsPathFullyQualified(path)
            ? path
            : Path.Combine(Directory.GetCurrentDirectory(), path);
    }

    private static void EnsureDirectory(string filePath)
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
