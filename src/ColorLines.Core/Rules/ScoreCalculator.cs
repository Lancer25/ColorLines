namespace ColorLines.Core.Rules;

public static class ScoreCalculator
{
    public static int Calculate(int lineCount, int clearedPieces)
    {
        if (lineCount <= 0 || clearedPieces < 5)
        {
            return 0;
        }

        var baseScore = 10;
        var extraPieceScore = Math.Max(0, clearedPieces - 5) * 4;
        var multiLineBonus = Math.Max(0, lineCount - 1) * 10;
        return baseScore + extraPieceScore + multiLineBonus;
    }
}
