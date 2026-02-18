using System.Collections.ObjectModel;
using System.Windows.Input;
using BulkRenamer.Core.Enums;
using BulkRenamer.Core.Models;
using BulkRenamer.Core.Services;
using BulkRenamer.ViewModels.Base;

namespace BulkRenamer.ViewModels;

/// ViewModel for the main window. Holds all UI state and exposes commands.
public sealed class MainViewModel : ViewModelBase
{
    private readonly IRenameService     _renameService;
    private readonly IFileSystemService _fileSystemService;

    private string _folderPath = string.Empty;
    public string FolderPath
    {
        get => _folderPath;
        set
        {
            if (SetProperty(ref _folderPath, value))
                RebuildPreview();
        }
    }

    private bool _includeSubfolders = true;
    public bool IncludeSubfolders
    {
        get => _includeSubfolders;
        set
        {
            if (SetProperty(ref _includeSubfolders, value))
                RebuildPreview();
        }
    }

    private bool _useFilter;
    public bool UseFilter
    {
        get => _useFilter;
        set
        {
            if (SetProperty(ref _useFilter, value))
                RebuildPreview();
        }
    }

    private MatchMode _filterMatch = MatchMode.Contains;
    public MatchMode FilterMatch
    {
        get => _filterMatch;
        set
        {
            if (SetProperty(ref _filterMatch, value))
                RebuildPreview();
        }
    }

    private string _filterText = string.Empty;
    public string FilterText
    {
        get => _filterText;
        set
        {
            if (SetProperty(ref _filterText, value))
                RebuildPreview();
        }
    }

    private ReplaceMode _replaceMode = ReplaceMode.PlainText;
    public ReplaceMode ReplaceMode
    {
        get => _replaceMode;
        set
        {
            if (SetProperty(ref _replaceMode, value))
                RebuildPreview();
        }
    }

    private ApplyMode _applyMode = ApplyMode.Anywhere;
    public ApplyMode ApplyMode
    {
        get => _applyMode;
        set
        {
            if (SetProperty(ref _applyMode, value))
                RebuildPreview();
        }
    }

    private bool _caseSensitive = true;
    public bool CaseSensitive
    {
        get => _caseSensitive;
        set
        {
            if (SetProperty(ref _caseSensitive, value))
                RebuildPreview();
        }
    }

    private string _findText = string.Empty;
    public string FindText
    {
        get => _findText;
        set
        {
            if (SetProperty(ref _findText, value))
                RebuildPreview();
        }
    }

    private string _replaceText = string.Empty;
    public string ReplaceText
    {
        get => _replaceText;
        set
        {
            if (SetProperty(ref _replaceText, value))
                RebuildPreview();
        }
    }

    private bool _previewOnly = true;
    public bool PreviewOnly
    {
        get => _previewOnly;
        set => SetProperty(ref _previewOnly, value);
    }

    private bool _skipIfNoChange = true;
    public bool SkipIfNoChange
    {
        get => _skipIfNoChange;
        set
        {
            if (SetProperty(ref _skipIfNoChange, value))
                RebuildPreview();
        }
    }

    private bool _skipIfNameCollision = true;
    public bool SkipIfNameCollision
    {
        get => _skipIfNameCollision;
        set
        {
            if (SetProperty(ref _skipIfNameCollision, value))
                RebuildPreview();
        }
    }

    private string _statusMessage = string.Empty;
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    private string _regexError = string.Empty;
    public string RegexError
    {
        get => _regexError;
        set => SetProperty(ref _regexError, value);
    }

    /// Observable collection so the ListView updates automatically when previews change.
    public ObservableCollection<RenamePreview> Previews { get; } = new();

    public ICommand BrowseFolderCommand  { get; }
    public ICommand RebuildPreviewCommand { get; }
    public ICommand ApplyRenameCommand   { get; }

    public MainViewModel(IRenameService renameService, IFileSystemService fileSystemService)
    {
        _renameService     = renameService;
        _fileSystemService = fileSystemService;

        BrowseFolderCommand   = new RelayCommand(BrowseFolder);
        RebuildPreviewCommand = new RelayCommand(RebuildPreview);
        ApplyRenameCommand    = new RelayCommand(ApplyRename, CanApplyRename);
    }

    private void BrowseFolder()
    {
        // FolderBrowserDialog lives in WPF/WinForms — we use the modern
        // Windows.Forms version since WPF has no built-in folder picker.
        var dialog = new System.Windows.Forms.FolderBrowserDialog
        {
            Description = "Select the folder to rename files in",
            UseDescriptionForTitle = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            FolderPath = dialog.SelectedPath;
        }
    }

    private void RebuildPreview()
    {
        Previews.Clear();
        RegexError     = string.Empty;
        StatusMessage  = string.Empty;

        if (!_fileSystemService.DirectoryExists(FolderPath))
        {
            StatusMessage = "Folder not found.";
            return;
        }

        var files    = _fileSystemService.GetFiles(FolderPath, IncludeSubfolders);
        var settings = BuildSettings();
        var previews = _renameService.BuildPreview(files, settings);

        foreach (var preview in previews)
        {
            Previews.Add(preview);
        }

        StatusMessage = $"{Previews.Count} file(s) in preview.";
        RelayCommand.RaiseCanExecuteChanged();
    }

    private void ApplyRename()
    {
        if (PreviewOnly)
        {
            StatusMessage = "Preview Only is ON — disable it to apply renames.";
            return;
        }

        var settings = BuildSettings();
        var renamed  = _renameService.ApplyRenames(Previews, settings);

        StatusMessage = $"Done. {renamed} file(s) renamed.";
        RebuildPreview();
    }

    private bool CanApplyRename() => Previews.Count > 0;

    private RenameSettings BuildSettings() => new()
    {
        FolderPath = FolderPath,
        IncludeSubfolders = IncludeSubfolders,
        UseFilter = UseFilter,
        FilterMatch = FilterMatch,
        FilterText = FilterText,
        ReplaceMode = ReplaceMode,
        ApplyMode = ApplyMode,
        CaseSensitive = CaseSensitive,
        FindText = FindText,
        ReplaceText = ReplaceText,
        SkipIfNoChange = SkipIfNoChange,
        SkipIfNameCollision = SkipIfNameCollision
    };
}
