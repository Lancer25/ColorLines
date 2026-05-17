namespace ColorLines.Core.Rules;

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random random;

    public SystemRandomSource()
        : this(Random.Shared)
    {
    }

    public SystemRandomSource(Random random)
    {
        this.random = random;
    }

    public int Next(int exclusiveMax)
    {
        return random.Next(exclusiveMax);
    }
}
