using System.ComponentModel;
using System.Runtime.CompilerServices;
using ColorLines.Core.Game;

namespace ColorLines.Windows.ViewModels;

public sealed class CellViewModel : INotifyPropertyChanged
{
    private bool isPathPreview;

    private CellViewModel(
        int row,
        int column,
        bool isOccupied,
        bool isSelected,
        string pieceLabel,
        string pieceName,
        PieceViewModel? piece,
        bool wasMovedTo,
        bool wasSpawned,
        bool wasCleared,
        bool wasRejectedTarget,
        bool isReachableTarget,
        bool isPathPreview)
    {
        Row = row;
        Column = column;
        IsOccupied = isOccupied;
        IsSelected = isSelected;
        PieceLabel = pieceLabel;
        PieceName = pieceName;
        Piece = piece;
        WasMovedTo = wasMovedTo;
        WasSpawned = wasSpawned;
        WasCleared = wasCleared;
        WasRejectedTarget = wasRejectedTarget;
        IsReachableTarget = isReachableTarget;
        this.isPathPreview = isPathPreview;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Row { get; }

    public int Column { get; }

    public bool IsOccupied { get; }

    public bool IsSelected { get; }

    public string PieceLabel { get; }

    public string PieceName { get; }

    public PieceViewModel? Piece { get; }

    public bool WasMovedTo { get; }

    public bool WasSpawned { get; }

    public bool WasCleared { get; }

    public bool WasRejectedTarget { get; }

    public bool IsReachableTarget { get; }

    public bool IsPathPreview
    {
        get => isPathPreview;
        private set
        {
            if (isPathPreview != value)
            {
                isPathPreview = value;
                OnPropertyChanged();
            }
        }
    }

    public static CellViewModel Empty(
        int row,
        int column,
        bool wasMovedTo = false,
        bool wasSpawned = false,
        bool wasCleared = false,
        bool wasRejectedTarget = false,
        bool isReachableTarget = false,
        bool isPathPreview = false)
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
            wasRejectedTarget,
            isReachableTarget,
            isPathPreview);
    }

    public static CellViewModel Occupied(
        int row,
        int column,
        PieceKind piece,
        bool isSelected,
        bool wasMovedTo = false,
        bool wasSpawned = false,
        bool wasCleared = false,
        bool wasRejectedTarget = false,
        bool isPathPreview = false)
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
            wasRejectedTarget,
            false,
            isPathPreview);
    }

    public void SetPathPreview(bool value)
    {
        IsPathPreview = value;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
