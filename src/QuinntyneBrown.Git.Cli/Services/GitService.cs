using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace QuinntyneBrown.Git.Cli.Services;

public class GitService : IGitService
{
    private readonly ILogger<GitService> _logger;

    public GitService(ILogger<GitService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> CloneAsync(string url, string targetPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cloning {Url} to {Path}", url, targetPath);

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

        process.StartInfo.ArgumentList.Add("clone");
        process.StartInfo.ArgumentList.Add(url);
        process.StartInfo.ArgumentList.Add(targetPath);

        process.Start();

        var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = await process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        if (process.ExitCode != 0)
        {
            _logger.LogError("git clone failed: {Error}", error);
            return false;
        }

        if (!string.IsNullOrWhiteSpace(output))
        {
            _logger.LogDebug("{Output}", output);
        }

        return true;
    }
}
