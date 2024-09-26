namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class DirectoryShortcut : Shortcut
{
    public string Path { get; init; }

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