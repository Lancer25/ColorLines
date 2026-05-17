using ColorLines.Windows.ViewModels;

namespace ColorLines.Tests;

public sealed class GameViewModelTests
{
    [Fact]
    public void RelayCommandRunsExecuteWhenAllowed()
    {
        var executed = false;
        var command = new RelayCommand(_ => executed = true, _ => true);

        Assert.True(command.CanExecute(null));
        command.Execute(null);

        Assert.True(executed);
    }

    [Fact]
    public void RelayCommandDoesNotRunExecuteWhenBlocked()
    {
        var executed = false;
        var command = new RelayCommand(_ => executed = true, _ => false);

        Assert.False(command.CanExecute(null));

        Assert.False(executed);
    }
}
