using System.CommandLine;
using Microsoft.Extensions.Hosting;
using QuinntyneBrown.Git.Cli.Commands;
using QuinntyneBrown.Git.Core;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddGitCore();
    })
    .Build();

var rootCommand = new RootCommand("Git CLI tool for managing temporary git repository clones");

rootCommand.AddCommand(CloneCommand.Create(host.Services));
rootCommand.AddCommand(CleanCommand.Create(host.Services));
rootCommand.AddCommand(PurgeCommand.Create(host.Services));

return await rootCommand.InvokeAsync(args);
