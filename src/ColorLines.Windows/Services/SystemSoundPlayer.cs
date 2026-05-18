using System.Media;

namespace ColorLines.Windows.Services;

public sealed class SystemSoundPlayer : ISoundPlayer
{
    public static SystemSoundPlayer Instance { get; } = new();

    private SystemSoundPlayer()
    {
    }

    public void Play(SoundCue cue)
    {
        switch (cue)
        {
            case SoundCue.Select:
                SystemSounds.Asterisk.Play();
                break;
            case SoundCue.Move:
                SystemSounds.Beep.Play();
                break;
            case SoundCue.Clear:
                SystemSounds.Exclamation.Play();
                break;
            case SoundCue.Reject:
                SystemSounds.Hand.Play();
                break;
            case SoundCue.GameOver:
                SystemSounds.Question.Play();
                break;
            case SoundCue.NewGame:
                SystemSounds.Asterisk.Play();
                break;
        }
    }
}
