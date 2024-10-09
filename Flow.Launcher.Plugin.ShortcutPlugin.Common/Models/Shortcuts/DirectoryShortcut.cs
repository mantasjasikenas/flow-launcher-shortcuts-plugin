namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class DirectoryShortcut : Shortcut
{
    public string Path
    {
        get; set;
    }

    public override object Clone()
    {
        return new DirectoryShortcut
        {
            Key = Key,
            Path = Path,
            Alias = Alias,
            Description = Description,
            Icon = Icon
        };
    }

    public override string ToString()
    {
        return Path;
    }
}