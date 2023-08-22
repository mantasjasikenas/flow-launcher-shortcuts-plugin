namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public class PluginShortcut : Shortcut
{
    public string PluginName { get; init; }

    public string RawQuery { get; init; }

    public override object Clone()
    {
        return new PluginShortcut
        {
            Key = Key,
            PluginName = PluginName,
            RawQuery = RawQuery
        };
    }

    public override string ToString()
    {
        return $"{PluginName} {RawQuery}";
    }
}