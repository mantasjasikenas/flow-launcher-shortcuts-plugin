namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class UrlShortcut : Shortcut
{
    public string Url { get; init; }

    public string App { get; init; }

    public override object Clone()
    {
        return new UrlShortcut
        {
            Key = Key,
            Url = Url
        };
    }

    public override string ToString()
    {
        return Url;
    }
}