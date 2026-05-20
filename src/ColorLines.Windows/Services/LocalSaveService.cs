using System.IO;
using System.Text.Json;

namespace ColorLines.Windows.Services;

public sealed class LocalSaveService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    private readonly string path;

    public LocalSaveService(string path)
    {
        this.path = path;
    }

    public static LocalSaveService CreateDefault()
    {
        var directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ColorLines");
        return new LocalSaveService(Path.Combine(directory, "save.json"));
    }

    public LocalSaveData? Load()
    {
        if (!File.Exists(path))
        {
            return null;
        }

        var json = File.ReadAllText(path);
        return JsonSerializer.Deserialize<LocalSaveData>(json, JsonOptions);
    }

    public void Save(LocalSaveData data)
    {
        var directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, JsonOptions);
        var tempPath = $"{path}.tmp";
        try
        {
            File.WriteAllText(tempPath, json);
            File.Move(tempPath, path, true);
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }
}
