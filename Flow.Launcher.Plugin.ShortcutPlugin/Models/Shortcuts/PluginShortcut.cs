namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public class PluginShortcut : Shortcut
{
    public string ActionKeyword { get; init; }

    public string Query { get; init; }

    public override object Clone()
    {
        return new PluginShortcut
        {
            Key = Key,
            ActionKeyword = ActionKeyword,
            Query = Query
        };
    }

    public override string ToString()
    {
        return $"{ActionKeyword} {Query}";
    }
}