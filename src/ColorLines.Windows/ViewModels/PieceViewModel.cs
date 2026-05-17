using System.Windows.Media;
using ColorLines.Core.Game;

namespace ColorLines.Windows.ViewModels;

public sealed record PieceViewModel(
    PieceKind Kind,
    string Name,
    string Label,
    string FaceText,
    Brush BodyBrush,
    Brush FaceBrush,
    Brush EarBrush)
{
    public static PieceViewModel FromPiece(PieceKind piece)
    {
        var bodyBrush = ToBodyBrush(piece);
        var faceBrush = piece is PieceKind.Black or PieceKind.Tuxedo or PieceKind.BlueGray
            ? Brushes.White
            : Brushes.SaddleBrown;

        return new PieceViewModel(
            piece,
            piece.ToString(),
            ToLabel(piece),
            "=^.^=",
            bodyBrush,
            faceBrush,
            bodyBrush);
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

    private static Brush ToBodyBrush(PieceKind piece)
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
