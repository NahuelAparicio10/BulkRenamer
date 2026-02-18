using BulkRenamer.Core.Enums;

namespace BulkRenamer.Core.Models;

// Plain data object that holds the full configuration for a rename operation.
// Passed from the ViewModel into the service layer — keeps the service stateless and testable.
public sealed class RenameSettings
{
    #region Scope

    public ScopeMode Scope { get; init; } = ScopeMode.Folder;

    // Absolute path of the root folder to scan.
    public string FolderPath { get; init; } = string.Empty;

    public bool IncludeSubfolders { get; init; } = true;

    #endregion

    #region Filter

    public bool UseFilter { get; init; }
    public MatchMode FilterMatch { get; init; } = MatchMode.Contains;
    public string FilterText { get; init; } = string.Empty;

    #endregion

    #region Replace

    public ReplaceMode ReplaceMode { get; init; } = ReplaceMode.PlainText;
    public ApplyMode ApplyMode { get; init; } = ApplyMode.Anywhere;
    public bool CaseSensitive { get; init; } = true;
    public string FindText { get; init; } = string.Empty;
    public string ReplaceText { get; init; } = string.Empty;

    #endregion

    #region Safety

    public bool SkipIfNoChange { get; init; } = true;
    public bool SkipIfNameCollision { get; init; } = true;

    #endregion

}

