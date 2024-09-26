namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class FileShortcut : Shortcut
{
    public string Path { get; init; }

    public string App { get; init; }

    public override object Clone()
    {
        return new FileShortcut
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