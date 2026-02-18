using System.Windows;
using Application = System.Windows.Application;
using BulkRenamer.Core.Services;
using BulkRenamer.ViewModels;
using BulkRenamer.Views;

namespace BulkRenamer;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Manual dependency injection — no DI container needed for an app this size.
        // If it grows, swap this for Microsoft.Extensions.DependencyInjection.
        var renameService = new RenameService();
        var fileSystemService = new FileSystemService();
        var viewModel = new MainViewModel(renameService, fileSystemService);

        var mainWindow = new MainWindow { DataContext = viewModel };
        mainWindow.Show();
    }
}
