using ColorLines.Core.Storage;

namespace ColorLines.Windows.Services;

public sealed record WindowPlacementData(double Width, double Height);

public sealed record LocalSaveData(
    int Version,
    int HighScore,
    bool IsSoundEnabled,
    string AnimationIntensity,
    string ThemeId,
    GameSnapshot? Game,
    WindowPlacementData Window)
{
    public string Difficulty { get; init; } = "Normal";

    public string Language { get; init; } = "en";
}
