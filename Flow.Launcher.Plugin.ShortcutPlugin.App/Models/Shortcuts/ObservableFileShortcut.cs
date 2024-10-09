using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public class ObservableFileShortcut : ObservableShortcut
{
    private readonly FileShortcut _fileShortcut;

    public ObservableFileShortcut(FileShortcut fileShortcut) : base(fileShortcut)
    {
        _fileShortcut = fileShortcut;
    }

    public string Path
    {
        get => _fileShortcut.Path;
        set
        {
            if (_fileShortcut.Path != value)
            {
                _fileShortcut.Path = value;
                OnPropertyChanged();
            }
        }
    }

    public string App
    {
        get => _fileShortcut.App;
        set
        {
            if (_fileShortcut.App != value)
            {
                _fileShortcut.App = value;
                OnPropertyChanged();
            }
        }
    }

    public new ObservableFileShortcut Clone()
    {
        return new ObservableFileShortcut((FileShortcut)_fileShortcut.Clone());
    }

    public override string ToString()
    {
        return _fileShortcut.ToString();
    }
}