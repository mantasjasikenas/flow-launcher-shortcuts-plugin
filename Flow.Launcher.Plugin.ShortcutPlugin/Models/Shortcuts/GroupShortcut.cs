using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public class GroupShortcut : Shortcut
{
    [CanBeNull] public List<Shortcut> Shortcuts { get; init; }

    [CanBeNull] public List<string> Keys { get; init; }

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