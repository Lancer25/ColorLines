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

    [Fact]
    public void CellViewModelShowsEmptyCell()
    {
        var cell = CellViewModel.Empty(4, 5);

        Assert.Equal(4, cell.Row);
        Assert.Equal(5, cell.Column);
        Assert.False(cell.IsOccupied);
        Assert.Equal(string.Empty, cell.PieceLabel);
    }

    [Fact]
    public void CellViewModelShowsOccupiedCatPiece()
    {
        var cell = CellViewModel.Occupied(1, 2, ColorLines.Core.Game.PieceKind.Calico, false);

        Assert.True(cell.IsOccupied);
        Assert.Equal("C", cell.PieceLabel);
        Assert.Equal("Calico", cell.PieceName);
    }
}
