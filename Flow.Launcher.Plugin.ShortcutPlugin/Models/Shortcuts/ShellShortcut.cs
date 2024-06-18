using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public class ShellShortcut : Shortcut
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ShellType ShellType { get; init; }

    public string Arguments { get; init; }
    public bool Silent { get; init; } = true;

    public override object Clone()
    {
        return new ShellShortcut
        {
            Key = Key,
            Arguments = Arguments,
            ShellType = ShellType,
            Silent = Silent
        };
    }

    public override string ToString()
    {
        return $"{Arguments} ({ShellType.ToString().ToLower()})";
    }
}