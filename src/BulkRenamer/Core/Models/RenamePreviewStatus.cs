namespace BulkRenamer.Core.Models;
public enum RenamePreviewStatus
{
    WillRename, // Name will change — ready to apply.
    NoChange, // The computed new name is identical to the old name — no action needed.
    Collision // Another file with the target name already exists in the same folder.
}

