namespace QuinntyneBrown.Git.Cli.Services;

public interface IGitService
{
    Task<bool> CloneAsync(string url, string targetPath, CancellationToken cancellationToken = default);
}
