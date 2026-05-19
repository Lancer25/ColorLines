namespace ColorLines.Windows.ViewModels;

public static class DifficultyCatalog
{
    public const string Easy = "Easy";
    public const string Normal = "Normal";
    public const string Hard = "Hard";

    public static int ToBoardSize(string? difficulty)
    {
        return difficulty switch
        {
            Easy => 7,
            Hard => 11,
            _ => 9
        };
    }

    public static string Normalize(string? difficulty)
    {
        return difficulty switch
        {
            Easy => Easy,
            Hard => Hard,
            _ => Normal
        };
    }
}
