namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public class ProgramShortcut : Shortcut
{
    public string Path { get; set; }

    public override object Clone()
    {
        return new ProgramShortcut
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