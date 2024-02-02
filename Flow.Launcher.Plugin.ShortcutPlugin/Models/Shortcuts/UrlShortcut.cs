namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

public class UrlShortcut : Shortcut
{
    public string Url { get; set; }

    public string App { get; set; }

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