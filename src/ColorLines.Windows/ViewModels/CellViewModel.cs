using ColorLines.Core.Game;

namespace ColorLines.Windows.ViewModels;

public sealed record CellViewModel(
    int Row,
    int Column,
    bool IsOccupied,
    bool IsSelected,
    string PieceLabel,
    string PieceName,
    PieceViewModel? Piece,
    bool WasMovedTo,
    bool WasSpawned,
    bool WasCleared,
    bool WasRejectedTarget)
{
    public static CellViewModel Empty(
        int row,
        int column,
        bool wasMovedTo = false,
        bool wasSpawned = false,
        bool wasCleared = false,
        bool wasRejectedTarget = false)
    {
        return new CellViewModel(
            row,
            column,
            false,
            false,
            string.Empty,
            string.Empty,
            null,
            wasMovedTo,
            wasSpawned,
            wasCleared,
            wasRejectedTarget);
    }

    public static CellViewModel Occupied(
        int row,
        int column,
        PieceKind piece,
        bool isSelected,
        bool wasMovedTo = false,
        bool wasSpawned = false,
        bool wasCleared = false,
        bool wasRejectedTarget = false)
    {
        var viewModel = PieceViewModel.FromPiece(piece);
        return new CellViewModel(
            row,
            column,
            true,
            isSelected,
            viewModel.Label,
            viewModel.Name,
            viewModel,
            wasMovedTo,
            wasSpawned,
            wasCleared,
            wasRejectedTarget);
    }
}
