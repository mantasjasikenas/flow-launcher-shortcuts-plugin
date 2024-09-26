using System.Text.Json;
using System.Text.Json.Serialization;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;

public static class ShortcutUtilities
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task<Dictionary<string, List<Shortcut>>> ReadShortcuts(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, List<Shortcut>>();
        }

        await using var openStream = File.OpenRead(path);
        var shortcuts = await JsonSerializer.DeserializeAsync<List<Shortcut>>(openStream);

        return shortcuts
               .SelectMany(s => (s.Alias ?? Enumerable.Empty<string>()).Append(s.Key),
                   (s, k) => new {Shortcut = s, Key = k})
               .GroupBy(x => x.Key)
               .ToDictionary(x => x.Key, x => x.Select(y => y.Shortcut).ToList());
    }

    public static void SaveShortcuts(Dictionary<string, List<Shortcut>> shortcuts, string shortcutPath)
    {
        var flattenShortcuts = shortcuts.Values
                                        .SelectMany(x => x)
                                        .Distinct()
                                        .ToList();

        var json = JsonSerializer.Serialize(flattenShortcuts, WriteOptions);
        var directory = Path.GetDirectoryName(shortcutPath);

        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(shortcutPath, json);
    }
}