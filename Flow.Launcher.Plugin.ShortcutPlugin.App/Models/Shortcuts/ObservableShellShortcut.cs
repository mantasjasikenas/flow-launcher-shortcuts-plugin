using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public class ObservableShellShortcut : ObservableShortcut
{
    private readonly ShellShortcut _shellShortcut;

    public ObservableShellShortcut(ShellShortcut shellShortcut) : base(shellShortcut)
    {
        _shellShortcut = shellShortcut;
    }

    public ShellType ShellType
    {
        get => _shellShortcut.ShellType;
        set
        {
            if (_shellShortcut.ShellType != value)
            {
                _shellShortcut.ShellType = value;
                OnPropertyChanged();
            }
        }
    }

    public string Arguments
    {
        get => _shellShortcut.Arguments;
        set
        {
            if (_shellShortcut.Arguments != value)
            {
                _shellShortcut.Arguments = value;
                OnPropertyChanged();
            }
        }
    }

    public bool Silent
    {
        get => _shellShortcut.Silent;
        set
        {
            if (_shellShortcut.Silent != value)
            {
                _shellShortcut.Silent = value;
                OnPropertyChanged();
            }
        }
    }

    public new ObservableShellShortcut Clone()
    {
        return new ObservableShellShortcut((ShellShortcut)_shellShortcut.Clone());
    }

    public override string ToString()
    {
        return _shellShortcut.ToString();
    }
}