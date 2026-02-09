using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QuinntyneBrown.Git.Cli.Commands;
using QuinntyneBrown.Git.Cli.Services;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<ITempFolderService, TempFolderService>();
    })
    .Build();

var rootCommand = new RootCommand("Git CLI tool for managing temporary git repository clones");

rootCommand.AddCommand(CloneCommand.Create(host.Services));
rootCommand.AddCommand(CleanCommand.Create(host.Services));
rootCommand.AddCommand(PurgeCommand.Create(host.Services));

return await rootCommand.InvokeAsync(args);
