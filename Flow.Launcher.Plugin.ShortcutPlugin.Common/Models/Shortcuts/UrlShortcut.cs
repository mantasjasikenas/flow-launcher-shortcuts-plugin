namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

public class UrlShortcut : Shortcut
{
    public string Url { get; set; }

    public string App { get; set; }

    public override object Clone()
    {
        return new UrlShortcut
        {
            Key = Key,
            Url = Url,
            App = App,
            Alias = Alias,
            Description = Description,
            Icon = Icon
        };
    }

    public override string ToString()
    {
        return Url;
    }
}