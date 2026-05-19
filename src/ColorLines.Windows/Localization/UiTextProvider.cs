namespace ColorLines.Windows.Localization;

public sealed class UiTextProvider
{
    public UiTextProvider(string? language = null)
    {
        Language = Normalize(language);
    }

    public string Language { get; private set; }

    public bool IsChinese => Language == "zh";

    public void SetLanguage(string? language)
    {
        Language = Normalize(language);
    }

    public string SelectCatToMove => IsChinese ? "选择一只猫咪移动。" : "Select a cat to move.";

    public string SelectCatBeforeTarget => IsChinese ? "先选择一只猫咪，再选择目标格。" : "Select a cat before choosing a target.";

    public string CannotMoveThere => IsChinese ? "这只猫咪不能移动到那里。" : "That cat cannot move there.";

    public string GameSaved => IsChinese ? "游戏已保存。" : "Game saved.";

    public string GameOverStatus => IsChinese ? "游戏结束。开始新游戏？" : "Game over. Start a new game?";

    public string GameOverTitle => IsChinese ? "游戏结束" : "Game Over";

    public string GameOverSummary => IsChinese ? "棋盘已满。开始新一局？" : "The board is full. Start a new run?";

    public string UseReducedAnimation => IsChinese ? "使用精简动效" : "Use Reduced Animation";

    public string UseFullAnimation => IsChinese ? "使用完整动效" : "Use Full Animation";

    public string ContinueSummary(int score, int highScore)
    {
        return IsChinese
            ? $"继续游戏：分数 {score} | 最高 {highScore}"
            : $"Continue available: Score {score} | Best {highScore}";
    }

    public string NewGameSummary(int score, int highScore)
    {
        return IsChinese
            ? $"开始新游戏：分数 {score} | 最高 {highScore}"
            : $"Start a new game: Score {score} | Best {highScore}";
    }

    public string SelectedPiece(string piece)
    {
        return IsChinese
            ? $"已选择 {piece}。请选择空格。"
            : $"Selected {piece}. Choose an empty cell.";
    }

    public string Points(int scoreDelta)
    {
        return IsChinese ? $"+{scoreDelta} 分！" : $"+{scoreDelta} points!";
    }

    public string FinalScore(int score)
    {
        return IsChinese ? $"最终分数：{score}" : $"Final Score: {score}";
    }

    public string BestScore(int highScore)
    {
        return IsChinese ? $"最高分：{highScore}" : $"Best Score: {highScore}";
    }

    private static string Normalize(string? language)
    {
        return string.Equals(language, "zh", StringComparison.OrdinalIgnoreCase) ? "zh" : "en";
    }
}
