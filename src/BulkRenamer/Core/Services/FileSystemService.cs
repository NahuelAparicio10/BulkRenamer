using System.IO;
namespace BulkRenamer.Core.Services;

public sealed class FileSystemService : IFileSystemService
{
    public IReadOnlyList<string> GetFiles(string folderPath, bool includeSubfolders, string extensionFilter = "")
    {
        if (!Directory.Exists(folderPath)) return Array.Empty<string>();

        var searchOption = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        // EnumerateFiles is lazy — avoids loading all paths into memory before filtering.
        var files = Directory.EnumerateFiles(folderPath, "*", searchOption);

        if (!string.IsNullOrWhiteSpace(extensionFilter))
        {
            // Parse comma-separated extensions: ".mp3, .wav, .ogg" → [".mp3", ".wav", ".ogg"]
            var extensions = extensionFilter.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(e => e.StartsWith('.') ? e : "." + e).ToHashSet(StringComparer.OrdinalIgnoreCase);

            files = files.Where(f => extensions.Contains(Path.GetExtension(f)));
        }

        return files.ToList();
    }

    public bool DirectoryExists(string path) => Directory.Exists(path);
}
