using Microsoft.Extensions.DependencyInjection;
using QuinntyneBrown.Git.Core.Services;

namespace QuinntyneBrown.Git.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGitCore(this IServiceCollection services)
    {
        services.AddSingleton<IGitService, GitService>();
        services.AddSingleton<ITempFolderService, TempFolderService>();
        return services;
    }
}
