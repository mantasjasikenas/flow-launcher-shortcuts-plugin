using System.Text.Json.Serialization;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public enum ShellType
{
    Cmd,
    Powershell
}

public class ShellShortcut : Shortcut
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ShellType ShellType { get; set; }

    public string Arguments { get; set; }
    public bool Silent { get; set; } = true;

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