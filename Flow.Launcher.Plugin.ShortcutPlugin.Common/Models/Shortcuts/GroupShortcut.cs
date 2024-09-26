#nullable enable
namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class GroupShortcut : Shortcut
{
    public List<Shortcut>? Shortcuts { get; init; }

    public List<string>? Keys { get; init; }

    public bool GroupLaunch { get; init; } = true;

    public override object Clone()
    {
        return new GroupShortcut
        {
            Key = Key,
            Shortcuts = Shortcuts,
            Keys = Keys
        };
    }

    public override string ToString()
    {
        var shortcuts = Shortcuts?.Select(x => x.ToString()).Aggregate((x, y) => x + ", " + y) ?? string.Empty;
        var keys = Keys?.Aggregate((x, y) => x + ", " + y) ?? string.Empty;

        return $"{shortcuts}{(string.IsNullOrEmpty(shortcuts) ? string.Empty : ", ")}{keys}".Trim();
    }
}