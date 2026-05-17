namespace ColorLines.Core.Rules;

public interface IRandomSource
{
    int Next(int exclusiveMax);
}
