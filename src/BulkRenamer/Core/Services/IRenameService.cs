using BulkRenamer.Core.Models;

namespace BulkRenamer.Core.Services;

// Defines the contract for the rename business logic.
public interface IRenameService
{
    // Computes what each file would be renamed to without touching the file system.
    /// <param name="files">Absolute paths of the candidate files.</param>
    /// <param name="settings">Configuration that drives filter and replace logic.</param>
    /// <returns>One preview entry per file that passes the filter.</returns>
    IReadOnlyList<RenamePreview> BuildPreview(IEnumerable<string> files, RenameSettings settings);

    // Applies renames to the file system based on already-computed previews.
    /// <param name="previews">Previews produced by <see cref="BuildPreview"/>.</param>
    /// <param name="settings">Settings used to honour safety flags (collision, no-change).</param>
    /// <returns>Number of files successfully renamed.</returns>
    int ApplyRenames(IReadOnlyList<RenamePreview> previews, RenameSettings settings);
}
