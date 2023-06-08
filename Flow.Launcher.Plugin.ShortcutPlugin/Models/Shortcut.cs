using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.ShortcutPlugin.models;

public class Shortcut
{
    public string Key { get; init; }

    public string Path { get; init; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ShortcutType Type { get; init; }
}