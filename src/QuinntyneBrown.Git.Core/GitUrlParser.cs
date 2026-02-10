namespace QuinntyneBrown.Git.Core;

public static class GitUrlParser
{
    /// <summary>
    /// Parses a full repository URL that may contain a branch path segment
    /// (e.g. https://github.com/user/repo/tree/feature-branch) and returns
    /// the base clone URL and the inferred branch name.
    /// </summary>
    public static (string CloneUrl, string? Branch) Parse(string url)
    {
        // Handle GitHub/GitLab/Bitbucket style URLs with /tree/<branch> or /src/<branch>
        var separators = new[] { "/tree/", "/src/" };

        foreach (var separator in separators)
        {
            var index = url.IndexOf(separator, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                var cloneUrl = url[..index];
                var branch = url[(index + separator.Length)..].TrimEnd('/');

                // Branch may contain additional path segments (e.g. /tree/branch/src/file)
                // We only want the branch name, not sub-paths within the repo
                var slashIndex = branch.IndexOf('/');
                if (slashIndex >= 0)
                {
                    branch = branch[..slashIndex];
                }

                if (!string.IsNullOrWhiteSpace(branch))
                {
                    return (cloneUrl, branch);
                }
            }
        }

        return (url.TrimEnd('/'), null);
    }
}
