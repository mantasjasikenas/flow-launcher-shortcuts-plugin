using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public class ObservableUrlShortcut : ObservableShortcut
{
    private readonly UrlShortcut _urlShortcut;

    public ObservableUrlShortcut(UrlShortcut urlShortcut) : base(urlShortcut)
    {
        _urlShortcut = urlShortcut;
    }

    public string Url
    {
        get => _urlShortcut.Url;
        set
        {
            if (_urlShortcut.Url != value)
            {
                _urlShortcut.Url = value;
                OnPropertyChanged();
            }
        }
    }

    public string App
    {
        get => _urlShortcut.App;
        set
        {
            if (_urlShortcut.App != value)
            {
                _urlShortcut.App = value;
                OnPropertyChanged();
            }
        }
    }

    public new ObservableUrlShortcut Clone()
    {
        return new ObservableUrlShortcut((UrlShortcut)_urlShortcut.Clone());
    }

    public override string ToString()
    {
        return _urlShortcut.ToString();
    }
}