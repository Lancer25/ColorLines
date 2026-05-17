namespace ColorLines.Windows.ViewModels;

public sealed record TurnFeedback(
    bool WasMoved,
    bool WasRejected,
    bool HadSpawn,
    bool HadClear,
    bool IsGameOver,
    int ScoreDelta)
{
    public static TurnFeedback Neutral { get; } = new(false, false, false, false, false, 0);

    public bool HasScore => ScoreDelta > 0;
}
