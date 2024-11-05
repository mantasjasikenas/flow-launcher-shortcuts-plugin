using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Core.Contracts.Services;


namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Services;

public class FileService : IFileService
{
    private static readonly JsonSerializerOptions jsonSerializerOptions = new()
    {
        WriteIndented = true
    };

    public T Read<T>(string folderPath, string fileName)
    {
        var path = Path.Combine(folderPath, fileName);

        if (File.Exists(path))
        {
            var json = File.ReadAllText(path);

            return JsonSerializer.Deserialize<T>(json);
        }

        return default;
    }

    public void Save<T>(string folderPath, string fileName, T content)
    {
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        var fileContent = JsonSerializer.Serialize(content, options: jsonSerializerOptions);

        File.WriteAllText(Path.Combine(folderPath, fileName), fileContent);
    }

    public void Delete(string folderPath, string fileName)
    {
        if (fileName != null && File.Exists(Path.Combine(folderPath, fileName)))
        {
            File.Delete(Path.Combine(folderPath, fileName));
        }
    }
}
