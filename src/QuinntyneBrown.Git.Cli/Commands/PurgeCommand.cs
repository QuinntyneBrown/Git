using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QuinntyneBrown.Git.Core.Services;

namespace QuinntyneBrown.Git.Cli.Commands;

public static class PurgeCommand
{
    public static Command Create(IServiceProvider services)
    {
        var command = new Command("purge", "Delete all temporary folders created by the tool");

        command.SetHandler(() =>
        {
            var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("Purge");
            var tempFolderService = services.GetRequiredService<ITempFolderService>();

            tempFolderService.DeleteAllFolders();
            Console.WriteLine("All temporary folders have been purged");
        });

        return command;
    }
}
