using System.IO;
using System.Media;

namespace ColorLines.Windows.Services;

public sealed class SystemSoundPlayer : ISoundPlayer
{
    private const int SampleRate = 22050;
    private static readonly IReadOnlyDictionary<SoundCue, byte[]> Sounds = new Dictionary<SoundCue, byte[]>
    {
        [SoundCue.Select] = CreateTone(660, 46, 0.20),
        [SoundCue.Move] = CreateTone(520, 58, 0.18),
        [SoundCue.Clear] = CreateChord(new[] { 660.0, 880.0 }, 110, 0.16),
        [SoundCue.Reject] = CreateTone(220, 62, 0.15),
        [SoundCue.GameOver] = CreateChord(new[] { 330.0, 247.0 }, 130, 0.15),
        [SoundCue.NewGame] = CreateChord(new[] { 523.0, 784.0 }, 90, 0.14)
    };

    public static SystemSoundPlayer Instance { get; } = new();

    private SystemSoundPlayer()
    {
    }

    public void Play(SoundCue cue)
    {
        if (Sounds.TryGetValue(cue, out var bytes))
        {
            using var stream = new MemoryStream(bytes);
            using var player = new SoundPlayer(stream);
            player.PlaySync();
        }
    }

    private static byte[] CreateTone(double frequency, int milliseconds, double volume)
    {
        return CreateChord(new[] { frequency }, milliseconds, volume);
    }

    private static byte[] CreateChord(IReadOnlyList<double> frequencies, int milliseconds, double volume)
    {
        var sampleCount = SampleRate * milliseconds / 1000;
        using var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + sampleCount * 2);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)1);
        writer.Write(SampleRate);
        writer.Write(SampleRate * 2);
        writer.Write((short)2);
        writer.Write((short)16);
        writer.Write("data"u8.ToArray());
        writer.Write(sampleCount * 2);

        for (var index = 0; index < sampleCount; index++)
        {
            var t = (double)index / SampleRate;
            var envelope = GetEnvelope(index, sampleCount);
            var sample = 0.0;
            foreach (var frequency in frequencies)
            {
                sample += Math.Sin(2 * Math.PI * frequency * t);
            }

            sample /= frequencies.Count;
            writer.Write((short)(sample * envelope * volume * short.MaxValue));
        }

        return stream.ToArray();
    }

    private static double GetEnvelope(int index, int sampleCount)
    {
        var attack = Math.Max(1, sampleCount / 12);
        var release = Math.Max(1, sampleCount / 4);

        if (index < attack)
        {
            return (double)index / attack;
        }

        var releaseStart = sampleCount - release;
        if (index > releaseStart)
        {
            return Math.Max(0, (double)(sampleCount - index) / release);
        }

        return 1;
    }
}
