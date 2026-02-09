using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuinntyneBrown.Git.Cli.Services;

namespace QuinntyneBrown.Git.Cli.Commands;

public static class CleanCommand
{
    public static Command Create(IServiceProvider services)
    {
        var urlArgument = new Argument<string>("url", "The URL of the git repository to clean up");

        var command = new Command("clean", "Delete the temporary folder for a specific git repository")
        {
            urlArgument
        };

        command.SetHandler((string url) =>
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Clean");
            var tempFolderService = services.GetRequiredService<ITempFolderService>();

            if (!tempFolderService.FolderExists(url))
            {
                logger.LogWarning("No temporary folder found for {Url}", url);
                return;
            }

            tempFolderService.DeleteFolder(url);
            Console.WriteLine($"Cleaned temporary folder for {url}");
        }, urlArgument);

        return command;
    }
}
