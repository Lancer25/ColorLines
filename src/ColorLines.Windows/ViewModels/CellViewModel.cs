using ColorLines.Core.Game;

namespace ColorLines.Windows.ViewModels;

public sealed record CellViewModel(
    int Row,
    int Column,
    bool IsOccupied,
    bool IsSelected,
    string PieceLabel,
    string PieceName,
    PieceViewModel? Piece)
{
    public static CellViewModel Empty(int row, int column)
    {
        return new CellViewModel(row, column, false, false, string.Empty, string.Empty, null);
    }

    public static CellViewModel Occupied(int row, int column, PieceKind piece, bool isSelected)
    {
        var viewModel = PieceViewModel.FromPiece(piece);
        return new CellViewModel(row, column, true, isSelected, viewModel.Label, viewModel.Name, viewModel);
    }
}
