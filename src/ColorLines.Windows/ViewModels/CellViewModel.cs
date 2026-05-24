using System.ComponentModel;
using System.Runtime.CompilerServices;
using ColorLines.Core.Game;

namespace ColorLines.Windows.ViewModels;

public sealed class CellViewModel : INotifyPropertyChanged
{
    private bool isOccupied;
    private bool isSelected;
    private string pieceLabel;
    private string pieceName;
    private PieceViewModel? piece;
    private bool wasMovedTo;
    private bool wasMovePath;
    private bool wasSpawned;
    private bool wasCleared;
    private bool wasRejectedTarget;
    private bool isReachableTarget;
    private bool isClearOpportunity;
    private bool isRecommendedClearTarget;
    private bool isPathPreview;
    private bool isPathPreviewTarget;

    private CellViewModel(
        int row,
        int column,
        bool isOccupied,
        bool isSelected,
        string pieceLabel,
        string pieceName,
        PieceViewModel? piece,
        bool wasMovedTo,
        bool wasMovePath,
        bool wasSpawned,
        bool wasCleared,
        bool wasRejectedTarget,
        bool isReachableTarget,
        bool isClearOpportunity,
        bool isRecommendedClearTarget,
        bool isPathPreview,
        bool isPathPreviewTarget)
    {
        Row = row;
        Column = column;
        this.isOccupied = isOccupied;
        this.isSelected = isSelected;
        this.pieceLabel = pieceLabel;
        this.pieceName = pieceName;
        this.piece = piece;
        this.wasMovedTo = wasMovedTo;
        this.wasMovePath = wasMovePath;
        this.wasSpawned = wasSpawned;
        this.wasCleared = wasCleared;
        this.wasRejectedTarget = wasRejectedTarget;
        this.isReachableTarget = isReachableTarget;
        this.isClearOpportunity = isClearOpportunity;
        this.isRecommendedClearTarget = isRecommendedClearTarget;
        this.isPathPreview = isPathPreview;
        this.isPathPreviewTarget = isPathPreviewTarget;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public int Row { get; }

    public int Column { get; }

    public bool IsOccupied
    {
        get => isOccupied;
        private set => SetProperty(ref isOccupied, value);
    }

    public bool IsSelected
    {
        get => isSelected;
        private set => SetProperty(ref isSelected, value);
    }

    public string PieceLabel
    {
        get => pieceLabel;
        private set => SetProperty(ref pieceLabel, value);
    }

    public string PieceName
    {
        get => pieceName;
        private set => SetProperty(ref pieceName, value);
    }

    public PieceViewModel? Piece
    {
        get => piece;
        private set => SetProperty(ref piece, value);
    }

    public bool WasMovedTo
    {
        get => wasMovedTo;
        private set => SetProperty(ref wasMovedTo, value);
    }

    public bool WasMovePath
    {
        get => wasMovePath;
        private set => SetProperty(ref wasMovePath, value);
    }

    public bool WasSpawned
    {
        get => wasSpawned;
        private set => SetProperty(ref wasSpawned, value);
    }

    public bool WasCleared
    {
        get => wasCleared;
        private set => SetProperty(ref wasCleared, value);
    }

    public bool WasRejectedTarget
    {
        get => wasRejectedTarget;
        private set => SetProperty(ref wasRejectedTarget, value);
    }

    public bool IsReachableTarget
    {
        get => isReachableTarget;
        private set => SetProperty(ref isReachableTarget, value);
    }

    public bool IsClearOpportunity
    {
        get => isClearOpportunity;
        private set => SetProperty(ref isClearOpportunity, value);
    }

    public bool IsRecommendedClearTarget
    {
        get => isRecommendedClearTarget;
        private set => SetProperty(ref isRecommendedClearTarget, value);
    }

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

    public bool IsPathPreviewTarget
    {
        get => isPathPreviewTarget;
        private set => SetProperty(ref isPathPreviewTarget, value);
    }

    public static CellViewModel Empty(
        int row,
        int column,
        bool wasMovedTo = false,
        bool wasMovePath = false,
        bool wasSpawned = false,
        bool wasCleared = false,
        bool wasRejectedTarget = false,
        bool isReachableTarget = false,
        bool isClearOpportunity = false,
        bool isRecommendedClearTarget = false,
        bool isPathPreview = false,
        bool isPathPreviewTarget = false)
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
            wasMovePath,
            wasSpawned,
            wasCleared,
            wasRejectedTarget,
            isReachableTarget,
            isClearOpportunity,
            isRecommendedClearTarget,
            isPathPreview,
            isPathPreviewTarget);
    }

    public static CellViewModel Occupied(
        int row,
        int column,
        PieceKind piece,
        bool isSelected,
        string? themeId = null,
        bool wasMovedTo = false,
        bool wasMovePath = false,
        bool wasSpawned = false,
        bool wasCleared = false,
        bool wasRejectedTarget = false,
        bool isClearOpportunity = false,
        bool isRecommendedClearTarget = false,
        bool isPathPreview = false,
        bool isPathPreviewTarget = false)
    {
        var viewModel = PieceViewModel.FromPiece(piece, themeId);
        return new CellViewModel(
            row,
            column,
            true,
            isSelected,
            viewModel.Label,
            viewModel.Name,
            viewModel,
            wasMovedTo,
            wasMovePath,
            wasSpawned,
            wasCleared,
            wasRejectedTarget,
            false,
            isClearOpportunity,
            isRecommendedClearTarget,
            isPathPreview,
            isPathPreviewTarget);
    }

    public void SetPathPreview(bool value, bool isTarget)
    {
        IsPathPreview = value;
        IsPathPreviewTarget = isTarget;
    }

    public void Update(
        PieceKind? pieceKind,
        bool isSelected,
        bool wasMovedTo,
        bool wasMovePath,
        bool wasSpawned,
        bool wasCleared,
        bool wasRejectedTarget,
        bool isReachableTarget,
        bool isClearOpportunity,
        bool isRecommendedClearTarget,
        bool isPathPreview,
        bool isPathPreviewTarget,
        string? themeId = null)
    {
        var nextPiece = pieceKind is null ? null : PieceViewModel.FromPiece(pieceKind.Value, themeId);
        IsOccupied = pieceKind is not null;
        IsSelected = pieceKind is not null && isSelected;
        PieceLabel = nextPiece?.Label ?? string.Empty;
        PieceName = nextPiece?.Name ?? string.Empty;
        Piece = nextPiece;
        WasMovedTo = wasMovedTo;
        WasMovePath = wasMovePath;
        WasSpawned = wasSpawned;
        WasCleared = wasCleared;
        WasRejectedTarget = wasRejectedTarget;
        IsReachableTarget = pieceKind is null && isReachableTarget;
        IsClearOpportunity = pieceKind is null && isClearOpportunity;
        IsRecommendedClearTarget = pieceKind is null && isRecommendedClearTarget;
        IsPathPreview = isPathPreview;
        IsPathPreviewTarget = pieceKind is null && isPathPreviewTarget;
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
