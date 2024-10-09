using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public class ObservableDirectoryShortcut : ObservableShortcut
{
    private readonly DirectoryShortcut _directoryShortcut;

    public ObservableDirectoryShortcut(DirectoryShortcut directoryShortcut) : base(directoryShortcut)
    {
        _directoryShortcut = directoryShortcut;
    }

    public string Path
    {
        get => _directoryShortcut.Path;
        set
        {
            if (_directoryShortcut.Path != value)
            {
                _directoryShortcut.Path = value;
                OnPropertyChanged();
            }
        }
    }

    public new ObservableDirectoryShortcut Clone()
    {
        return new ObservableDirectoryShortcut((DirectoryShortcut)_directoryShortcut.Clone());
    }

    public override string ToString()
    {
        return _directoryShortcut.ToString();
    }
}
