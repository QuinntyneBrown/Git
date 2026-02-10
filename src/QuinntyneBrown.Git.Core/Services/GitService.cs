using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace QuinntyneBrown.Git.Core.Services;

public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> CloneAsync(string url, string targetPath, string? branch = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cloning {Url} to {Path}", url, targetPath);

        var process = CreateGitProcess();

        process.StartInfo.ArgumentList.Add("clone");

        if (!string.IsNullOrWhiteSpace(branch))
        {
            process.StartInfo.ArgumentList.Add("--branch");
            process.StartInfo.ArgumentList.Add(branch);
        }

        process.StartInfo.ArgumentList.Add(url);
        process.StartInfo.ArgumentList.Add(targetPath);

        return await RunAsync(process, "git clone", cancellationToken);
    }

    public async Task<bool> CheckoutAsync(string repoPath, string reference, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking out {Reference} in {Path}", reference, repoPath);

        var process = CreateGitProcess(repoPath);
        process.StartInfo.ArgumentList.Add("checkout");
        process.StartInfo.ArgumentList.Add(reference);

        return await RunAsync(process, "git checkout", cancellationToken);
    }

    public async Task<bool> FetchAsync(string repoPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching all remotes in {Path}", repoPath);

        var process = CreateGitProcess(repoPath);
        process.StartInfo.ArgumentList.Add("fetch");
        process.StartInfo.ArgumentList.Add("--all");

        return await RunAsync(process, "git fetch", cancellationToken);
    }

    public async Task<string?> GetCurrentBranchAsync(string repoPath, CancellationToken cancellationToken = default)
    {
        var process = CreateGitProcess(repoPath);
        process.StartInfo.ArgumentList.Add("rev-parse");
        process.StartInfo.ArgumentList.Add("--abbrev-ref");
        process.StartInfo.ArgumentList.Add("HEAD");

        return await RunForOutputAsync(process, cancellationToken);
    }

    public async Task<string?> GetCurrentCommitAsync(string repoPath, CancellationToken cancellationToken = default)
    {
        var process = CreateGitProcess(repoPath);
        process.StartInfo.ArgumentList.Add("rev-parse");
        process.StartInfo.ArgumentList.Add("HEAD");

        return await RunForOutputAsync(process, cancellationToken);
    }

    public async Task<string?> DiffAsync(string repoPath, string fromRef, string toRef, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Diffing {From}..{To} in {Path}", fromRef, toRef, repoPath);

        var process = CreateGitProcess(repoPath);
        process.StartInfo.ArgumentList.Add("diff");
        process.StartInfo.ArgumentList.Add($"{fromRef}..{toRef}");

        return await RunForOutputAsync(process, cancellationToken);
    }

    private static Process CreateGitProcess(string? workingDirectory = null)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            }
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
        {
            process.StartInfo.WorkingDirectory = workingDirectory;
        }

        return process;
    }

    private async Task<bool> RunAsync(Process process, string commandName, CancellationToken cancellationToken)
    {
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogError("{Command} failed: {Error}", commandName, error);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(output))
        {
            _logger.LogDebug("{Output}", output);
        }

        return true;
    }

    private async Task<string?> RunForOutputAsync(Process process, CancellationToken cancellationToken)
    {
        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogError("git command failed: {Error}", error);
            return null;
        }

        return output.Trim();
    }
}
