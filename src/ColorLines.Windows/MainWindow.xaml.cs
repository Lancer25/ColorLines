using System.Windows;
using ColorLines.Core.Board;
using ColorLines.Core.Game;
using ColorLines.Core.Rules;

namespace ColorLines.Windows;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        var engine = new GameEngine(new SystemRandomSource());
        var state = engine.NewGame();
        DataContext = new MainWindowPreviewModel(state);
    }
}

public sealed class MainWindowPreviewModel
{
    public MainWindowPreviewModel(GameState state)
    {
        Score = state.Score;
        NextPiecesText = string.Join(", ", state.NextPieces);
        PreviewCells = state.Board.Cells()
            .Select(ToCellText)
            .ToArray();
    }

    public int Score { get; }

    public string NextPiecesText { get; }

    public IReadOnlyList<string> PreviewCells { get; }

    private static string ToCellText(Cell cell)
    {
        return cell.Piece switch
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
}
