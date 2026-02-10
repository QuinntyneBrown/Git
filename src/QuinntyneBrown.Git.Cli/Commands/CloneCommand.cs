using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuinntyneBrown.Git.Core.Services;

namespace QuinntyneBrown.Git.Cli.Commands;

public static class CloneCommand
{
    public static Command Create(IServiceProvider services)
    {
        var urlArgument = new Argument<string?>("url", () => null, "The URL of the git repository to clone");
        var branchOption = new Option<string?>("--branch", "The branch to clone or verify");
        branchOption.AddAlias("-b");
        var commitOption = new Option<string?>("--commit", "The commit to clone or verify");
        var tagOption = new Option<string?>("--tag", "A label to save or look up an existing clone");
        tagOption.AddAlias("-t");

        var command = new Command("clone", "Clone a git repository to a temporary folder")
        {
            urlArgument,
            branchOption,
            commitOption,
            tagOption
        };

        command.SetHandler(async (string? url, string? branch, string? commit, string? tag) =>
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Clone");
            var gitService = services.GetRequiredService<IGitService>();
            var tempFolderService = services.GetRequiredService<ITempFolderService>();

            // Resolve tag to URL when no URL is provided
            if (string.IsNullOrWhiteSpace(url))
            {
                if (string.IsNullOrWhiteSpace(tag))
                {
                    logger.LogError("Provide a URL or a --tag to look up an existing clone.");
                    return;
                }

                var resolved = tempFolderService.ResolveTag(tag);
                if (resolved == null)
                {
                    Console.Error.WriteLine($"Tag '{tag}' does not exist. Provide a URL to clone the repository:");
                    Console.Error.WriteLine($"  git-cli clone <url> --tag {tag}");
                    return;
                }

                url = resolved;
            }

            if (tempFolderService.FolderExists(url))
            {
                var existingPath = tempFolderService.GetFolderPath(url);

                if (await IsAtExpectedRef(gitService, existingPath, branch, commit))
                {
                    if (!string.IsNullOrWhiteSpace(tag))
                    {
                        tempFolderService.SaveTag(tag, url);
                    }

                    logger.LogInformation("Repository already cloned at {Path}", existingPath);
                    Console.WriteLine(existingPath);
                    return;
                }

                logger.LogInformation("Existing clone does not match requested branch/commit. Re-cloning.");
                tempFolderService.DeleteFolder(url);
            }

            tempFolderService.EnsureBasePath();
            var folder = tempFolderService.GetFolderPath(url);

            var success = await gitService.CloneAsync(url, folder, branch);

            if (success && !string.IsNullOrWhiteSpace(commit))
            {
                success = await gitService.CheckoutAsync(folder, commit);
            }

            if (success)
            {
                tempFolderService.WriteMarker(url);

                if (!string.IsNullOrWhiteSpace(tag))
                {
                    tempFolderService.SaveTag(tag, url);
                }

                Console.WriteLine(folder);
            }
            else
            {
                logger.LogError("Failed to clone repository");

                if (Directory.Exists(folder))
                {
                    Directory.Delete(folder, true);
                }
            }
        }, urlArgument, branchOption, commitOption, tagOption);

        return command;
    }

    private static async Task<bool> IsAtExpectedRef(IGitService gitService, string repoPath, string? branch, string? commit)
    {
        if (string.IsNullOrWhiteSpace(branch) && string.IsNullOrWhiteSpace(commit))
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(branch))
        {
            var currentBranch = await gitService.GetCurrentBranchAsync(repoPath);
            if (!string.Equals(currentBranch, branch, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        if (!string.IsNullOrWhiteSpace(commit))
        {
            var currentCommit = await gitService.GetCurrentCommitAsync(repoPath);
            if (currentCommit == null || !currentCommit.StartsWith(commit, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }
        }

        return true;
    }
}
