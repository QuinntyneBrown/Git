using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace QuinntyneBrown.Git.Cli.Services;

public class TempFolderService : ITempFolderService
{
    private readonly ILogger<TempFolderService> _logger;

    public TempFolderService(ILogger<TempFolderService> logger)
    {
        _logger = logger;
    }

    public string BasePath => Path.Combine(Path.GetTempPath(), "git-cli");

    public string GetFolderPath(string url)
    {
        var hash = GetHash(url);
        return Path.Combine(BasePath, hash);
    }

    public bool FolderExists(string url)
    {
        return Directory.Exists(GetFolderPath(url));
    }

    public void EnsureBasePath()
    {
        if (!Directory.Exists(BasePath))
        {
            Directory.CreateDirectory(BasePath);
            _logger.LogDebug("Created base path {Path}", BasePath);
        }
    }

    public void WriteMarker(string url)
    {
        var markerPath = Path.Combine(GetFolderPath(url), ".git-cli-url");
        File.WriteAllText(markerPath, url);
    }

    public void DeleteFolder(string url)
    {
        var path = GetFolderPath(url);

        if (!Directory.Exists(path))
        {
            _logger.LogWarning("No temporary folder found for {Url}", url);
            return;
        }

        ClearReadOnlyAttributes(new DirectoryInfo(path));
        Directory.Delete(path, true);
        _logger.LogInformation("Deleted {Path}", path);
    }

    public void DeleteAllFolders()
    {
        if (!Directory.Exists(BasePath))
        {
            _logger.LogInformation("No temporary folders to delete");
            return;
        }

        ClearReadOnlyAttributes(new DirectoryInfo(BasePath));
        Directory.Delete(BasePath, true);
        _logger.LogInformation("Deleted all temporary folders in {Path}", BasePath);
    }

    private static string GetHash(string url)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(url.ToLowerInvariant().TrimEnd('/')));
        return Convert.ToHexString(bytes)[..16].ToLowerInvariant();
    }

    private static void ClearReadOnlyAttributes(DirectoryInfo directory)
    {
        foreach (var sub in directory.GetDirectories())
        {
            ClearReadOnlyAttributes(sub);
        }

        foreach (var file in directory.GetFiles())
        {
            file.Attributes = FileAttributes.Normal;
        }
    }
}
