namespace BulkRenamer.Core.Models;
// Immutable snapshot of what a single file rename would look like before it is applied.
public readonly record struct RenamePreview(string FullPath, string OldName, string NewName, RenamePreviewStatus Status);


