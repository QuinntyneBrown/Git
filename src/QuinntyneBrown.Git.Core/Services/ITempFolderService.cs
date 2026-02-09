namespace QuinntyneBrown.Git.Core.Services;

public interface ITempFolderService
{
    string BasePath { get; }
    string GetFolderPath(string url);
    bool FolderExists(string url);
    void EnsureBasePath();
    void WriteMarker(string url);
    void DeleteFolder(string url);
    void DeleteAllFolders();
}
