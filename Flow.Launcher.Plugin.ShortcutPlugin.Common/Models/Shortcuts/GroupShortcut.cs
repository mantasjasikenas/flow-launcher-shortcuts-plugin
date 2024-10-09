#nullable enable
namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class GroupShortcut : Shortcut
{
    public List<Shortcut>? Shortcuts
    {
        get; set;
    }

    public List<string>? Keys
    {
        get; set;
    }

    public bool GroupLaunch { get; set; } = true;

    public override object Clone()
    {
        return new GroupShortcut
        {
            Key = Key,
            Shortcuts = Shortcuts,
            Keys = Keys,
            GroupLaunch = GroupLaunch,
            Alias = Alias,
            Description = Description,
            Icon = Icon
        };
    }

    public override string ToString()
    {
        var shortcuts = Shortcuts?.Select(x => x.ToString()).Aggregate((x, y) => x + ", " + y) ?? string.Empty;
        var keys = Keys?.Aggregate((x, y) => x + ", " + y) ?? string.Empty;

        return $"{shortcuts}{(string.IsNullOrEmpty(shortcuts) ? string.Empty : ", ")}{keys}".Trim();
    }
}