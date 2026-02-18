using BulkRenamer.Core.Models;

namespace BulkRenamer.Core.Services;

// Defines the contract for the rename business logic.
public interface IRenameService
{
    // Computes what each file would be renamed to without touching the file system.
    IReadOnlyList<RenamePreview> BuildPreview(IEnumerable<string> files, RenameSettings settings);

    // Applies renames to the file system based on already-computed previews.
    int ApplyRenames(IReadOnlyList<RenamePreview> previews, RenameSettings settings);
}
