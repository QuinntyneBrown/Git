using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuinntyneBrown.Git.Cli.Services;

namespace QuinntyneBrown.Git.Cli.Commands;

public static class CloneCommand
{
    public static Command Create(IServiceProvider services)
    {
        var urlArgument = new Argument<string>("url", "The URL of the git repository to clone");

        var command = new Command("clone", "Clone a git repository to a temporary folder")
        {
            urlArgument
        };

        command.SetHandler(async (string url) =>
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Clone");
            var gitService = services.GetRequiredService<IGitService>();
            var tempFolderService = services.GetRequiredService<ITempFolderService>();

            if (tempFolderService.FolderExists(url))
            {
                var existingPath = tempFolderService.GetFolderPath(url);
                logger.LogInformation("Repository already cloned at {Path}", existingPath);
                Console.WriteLine(existingPath);
                return;
            }

            tempFolderService.EnsureBasePath();
            var folder = tempFolderService.GetFolderPath(url);

            var success = await gitService.CloneAsync(url, folder);

            if (success)
            {
                tempFolderService.WriteMarker(url);
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
        }, urlArgument);

        return command;
    }
}
