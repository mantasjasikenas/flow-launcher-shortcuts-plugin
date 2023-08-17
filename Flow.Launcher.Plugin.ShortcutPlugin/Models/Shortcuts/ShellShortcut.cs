namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public class ShellShortcut : Shortcut
{
    public string Command { get; set; }

    public string TargetFilePath { get; set; }

    public bool Silent { get; set; }

    public override object Clone()
    {
        return new ShellShortcut
        {
            Key = Key,
            Command = Command,
            TargetFilePath = TargetFilePath,
            Silent = Silent
        };
    }

    public override string ToString()
    {
        return Command;
    }
}