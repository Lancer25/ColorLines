namespace ColorLines.Windows.Services;

public sealed class NullSoundPlayer : ISoundPlayer
{
    public static NullSoundPlayer Instance { get; } = new();

    private NullSoundPlayer()
    {
    }

    public void Play(SoundCue cue)
    {
    }
}
