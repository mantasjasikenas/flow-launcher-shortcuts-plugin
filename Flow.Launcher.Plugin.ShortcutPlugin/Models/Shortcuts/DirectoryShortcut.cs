using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public class DirectoryShortcut : Shortcut
{
    public string Path { get; set; }

    public override object Clone()
    {
        return new DirectoryShortcut
        {
            Key = Key,
            Path = Path
        };
    }

    public override string ToString()
    {
        return Path;
    }
}