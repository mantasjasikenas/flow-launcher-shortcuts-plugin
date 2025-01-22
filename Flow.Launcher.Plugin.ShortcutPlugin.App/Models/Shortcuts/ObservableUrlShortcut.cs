using System.ComponentModel.DataAnnotations;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public partial class ObservableUrlShortcut : ObservableShortcut
{
    private readonly UrlShortcut _urlShortcut;

    public ObservableUrlShortcut(UrlShortcut urlShortcut) : base(urlShortcut)
    {
        _urlShortcut = urlShortcut;
    }

    [Required(ErrorMessage = "Url is required")]
    public string Url
    {
        get => _urlShortcut.Url;
        set => SetProperty(_urlShortcut.Url, value, _urlShortcut, (u, n) => u.Url = n);
    }

    public string App
    {
        get => _urlShortcut.App;
        set => SetProperty(_urlShortcut.App, value, _urlShortcut, (u, n) => u.App = n);
    }

    public new ObservableUrlShortcut Clone()
    {
        return new ObservableUrlShortcut((UrlShortcut)_urlShortcut.Clone());
    }

    public override Shortcut GetBaseShortcut()
    {
        base.GetBaseShortcut();
        return _urlShortcut;
    }

    public override string ToString()
    {
        return _urlShortcut.ToString();
    }
}