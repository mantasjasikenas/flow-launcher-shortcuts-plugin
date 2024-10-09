namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class FileShortcut : Shortcut
{
    public string Path { get; set; }

    public string App { get; set; }

    public override object Clone()
    {
        return new FileShortcut
        {
            Key = Key,
            Path = Path ,
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