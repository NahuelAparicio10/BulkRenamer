using System.Windows.Input;

namespace BulkRenamer.ViewModels.Base;

// A command that delegates its execution and can-execute logic to lambdas.
public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Func<object?, bool>? _canExecute;

    public RelayCommand(Action<object?> execute, Func<object?, bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    /// Convenience constructor for commands that ignore the parameter.
    public RelayCommand(Action execute, Func<bool>? canExecute = null) : this(_ => execute(), canExecute is null ? null : _ => canExecute()) { }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

    public void Execute(object? parameter) => _execute(parameter);

    /// Forces WPF to re-evaluate <see cref="CanExecute"/> for all commands.
    // Call this after any state change that might affect button enabled states.
    public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
}
