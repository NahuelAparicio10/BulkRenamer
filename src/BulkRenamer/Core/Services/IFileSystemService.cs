namespace BulkRenamer.Core.Services;

// This makes the ViewModel testable without needing real files on disk.
public interface IFileSystemService
{
    /// Returns the absolute paths of all files inside <paramref name="folderPath"/>.
    IReadOnlyList<string> GetFiles(string folderPath, bool includeSubfolders, string extensionFilter = "");

    /// Returns true if <paramref name="path"/> points to an existing directory.
    bool DirectoryExists(string path);
}
