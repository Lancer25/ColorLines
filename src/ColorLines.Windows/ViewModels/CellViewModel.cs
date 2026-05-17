using System.Windows.Media;
using ColorLines.Core.Game;

namespace ColorLines.Windows.ViewModels;

public sealed record CellViewModel(
    int Row,
    int Column,
    bool IsOccupied,
    bool IsSelected,
    string PieceLabel,
    string PieceName,
    string FaceText,
    Brush PieceBrush)
{
    public static CellViewModel Empty(int row, int column)
    {
        return new CellViewModel(row, column, false, false, string.Empty, string.Empty, string.Empty, Brushes.Transparent);
    }

    public static CellViewModel Occupied(int row, int column, PieceKind piece, bool isSelected)
    {
        return new CellViewModel(row, column, true, isSelected, ToLabel(piece), piece.ToString(), "=^.^=", ToBrush(piece));
    }

    private static string ToLabel(PieceKind piece)
    {
        return piece switch
        {
            PieceKind.Orange => "O",
            PieceKind.Gray => "G",
            PieceKind.Tuxedo => "T",
            PieceKind.Calico => "C",
            PieceKind.Black => "B",
            PieceKind.White => "W",
            PieceKind.BlueGray => "N",
            _ => string.Empty
        };
    }

    private static Brush ToBrush(PieceKind piece)
    {
        return piece switch
        {
            PieceKind.Orange => Brushes.Orange,
            PieceKind.Gray => Brushes.Gray,
            PieceKind.Tuxedo => Brushes.DarkSlateGray,
            PieceKind.Calico => Brushes.SandyBrown,
            PieceKind.Black => Brushes.Black,
            PieceKind.White => Brushes.WhiteSmoke,
            PieceKind.BlueGray => Brushes.SteelBlue,
            _ => Brushes.Transparent
        };
    }
}
