namespace QuinntyneBrown.Git.Core.Services;

public interface IGitService
{
    Task<bool> CloneAsync(string url, string targetPath, string? branch = null, CancellationToken cancellationToken = default);
    Task<bool> CheckoutAsync(string repoPath, string reference, CancellationToken cancellationToken = default);
    Task<bool> FetchAsync(string repoPath, CancellationToken cancellationToken = default);
    Task<string?> GetCurrentBranchAsync(string repoPath, CancellationToken cancellationToken = default);
    Task<string?> GetCurrentCommitAsync(string repoPath, CancellationToken cancellationToken = default);
    Task<string?> DiffAsync(string repoPath, string fromRef, string toRef, CancellationToken cancellationToken = default);
}
