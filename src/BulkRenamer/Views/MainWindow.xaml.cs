using System.IO;
using System.Windows;
using BulkRenamer.ViewModels;
using DataFormats    = System.Windows.DataFormats;
using DragEventArgs  = System.Windows.DragEventArgs;
using DragDropEffects = System.Windows.DragDropEffects;

namespace BulkRenamer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        AllowDrop = true;
    }

    protected override void OnDragOver(DragEventArgs e)
    {
        // Show the copy cursor only when the dragged data contains file paths.
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

        e.Handled = true;
    }

    protected override void OnDrop(DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
        if (paths is not { Length: > 0 }) return;

        // Resolve the folder from the dropped item:
        // - If the user drops a folder, use it directly.
        // - If the user drops one or more files, use their parent folder.
        var folderPath = Directory.Exists(paths[0]) ? paths[0] : Path.GetDirectoryName(paths[0]) ?? string.Empty;

        if (string.IsNullOrEmpty(folderPath)) return;

        if (DataContext is MainViewModel vm)
        {
            vm.FolderPath = folderPath;
        }

        e.Handled = true;
    }
}
