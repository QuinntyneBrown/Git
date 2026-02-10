using System.Text;

namespace QuinntyneBrown.Git.Core;

public static class RepoContentAggregator
{
    private static readonly HashSet<string> SkipDirectories = new(StringComparer.OrdinalIgnoreCase)
    {
        ".git",
        ".vs",
        ".idea",
        "node_modules",
        "bin",
        "obj",
        "packages",
        ".nuget",
        "__pycache__",
        ".next",
        "dist",
        "coverage"
    };

    private static readonly HashSet<string> SkipExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".dll", ".pdb", ".bin", ".obj", ".o", ".a", ".lib", ".so", ".dylib",
        ".zip", ".tar", ".gz", ".7z", ".rar", ".bz2",
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".ico", ".svg", ".webp", ".tiff",
        ".mp3", ".mp4", ".avi", ".mov", ".wav", ".flac",
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".woff", ".woff2", ".ttf", ".eot",
        ".lock", ".cache",
        ".nupkg", ".snupkg"
    };

    /// <summary>
    /// Aggregates all text files in a repository into a single string
    /// formatted for efficient LLM consumption.
    /// </summary>
    public static async Task<string> AggregateAsync(string repoPath, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        var repoDir = new DirectoryInfo(repoPath);

        await AppendDirectoryAsync(sb, repoDir, repoDir.FullName, cancellationToken);

        return sb.ToString();
    }

    private static async Task AppendDirectoryAsync(StringBuilder sb, DirectoryInfo directory, string rootPath, CancellationToken cancellationToken)
    {
        foreach (var file in directory.EnumerateFiles())
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (ShouldSkipFile(file))
                continue;

            if (IsBinary(file.FullName))
                continue;

            var relativePath = Path.GetRelativePath(rootPath, file.FullName).Replace('\\', '/');

            try
            {
                var content = await File.ReadAllTextAsync(file.FullName, cancellationToken);
                sb.Append("<file path=\"").Append(relativePath).AppendLine("\">");
                sb.AppendLine(content.TrimEnd());
                sb.AppendLine("</file>");
            }
            catch
            {
                // Skip files that can't be read
            }
        }

        foreach (var sub in directory.EnumerateDirectories())
        {
            if (SkipDirectories.Contains(sub.Name))
                continue;

            await AppendDirectoryAsync(sb, sub, rootPath, cancellationToken);
        }
    }

    private static bool ShouldSkipFile(FileInfo file)
    {
        if (SkipExtensions.Contains(file.Extension))
            return true;

        // Skip hidden/system files and common non-text files
        if (file.Name.StartsWith('.') && file.Name != ".editorconfig" && file.Name != ".gitignore" && file.Name != ".gitattributes")
            return true;

        return false;
    }

    private static bool IsBinary(string filePath)
    {
        try
        {
            var buffer = new byte[512];
            using var stream = File.OpenRead(filePath);
            var bytesRead = stream.Read(buffer, 0, buffer.Length);

            for (var i = 0; i < bytesRead; i++)
            {
                if (buffer[i] == 0)
                    return true;
            }

            return false;
        }
        catch
        {
            return true;
        }
    }
}
