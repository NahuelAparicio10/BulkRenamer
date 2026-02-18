using System.IO;
namespace BulkRenamer.Core.Services;

public sealed class FileSystemService : IFileSystemService
{
    public IReadOnlyList<string> GetFiles(string folderPath, bool includeSubfolders)
    {
        if (!Directory.Exists(folderPath)) return Array.Empty<string>();

        var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        return Directory.EnumerateFiles(folderPath, "*", searchOption).ToList();
    }

    public bool DirectoryExists(string path) => Directory.Exists(path);
}
