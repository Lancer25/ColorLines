using System.Windows.Media;
using ColorLines.Core.Game;

namespace ColorLines.Windows.ViewModels;

public sealed record PieceViewModel(
    PieceKind Kind,
    string ThemeId,
    string Name,
    string Label,
    string FaceText,
    Brush BodyBrush,
    Brush FaceBrush,
    Brush EarBrush,
    Brush HighlightBrush,
    Brush ShadowBrush,
    Brush InnerEarBrush,
    string AssetPath)
{
    public static PieceViewModel FromPiece(PieceKind piece, string? themeId = null)
    {
        var normalizedThemeId = ColorLines.Windows.Themes.ThemeCatalog.Normalize(themeId);
        var bodyBrush = ToBodyBrush(piece);
        var faceBrush = piece is PieceKind.Black or PieceKind.Tuxedo or PieceKind.BlueGray
            ? Brushes.White
            : Brushes.SaddleBrown;

        return new PieceViewModel(
            piece,
            normalizedThemeId,
            piece.ToString(),
            ToLabel(piece),
            "=^.^=",
            bodyBrush,
            faceBrush,
            bodyBrush,
            Brushes.White,
            new SolidColorBrush(Color.FromArgb(80, 74, 45, 35)),
            Brushes.LightPink,
            ToAssetPath(piece, normalizedThemeId));
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

    private static string ToAssetPath(PieceKind piece, string themeId)
    {
        var fileName = piece switch
        {
            PieceKind.Orange => "orange",
            PieceKind.Gray => "gray",
            PieceKind.Tuxedo => "tuxedo",
            PieceKind.Calico => "calico",
            PieceKind.Black => "black",
            PieceKind.White => "white",
            PieceKind.BlueGray => "bluegray",
            _ => string.Empty
        };

        return $"/ColorLines.Windows;component/Assets/Themes/{themeId}/pieces/{fileName}.png";
    }
}
