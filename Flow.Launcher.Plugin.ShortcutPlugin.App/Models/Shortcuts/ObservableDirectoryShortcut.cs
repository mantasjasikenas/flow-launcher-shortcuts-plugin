using System.ComponentModel.DataAnnotations;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public class ObservableDirectoryShortcut : ObservableShortcut
{
    private readonly DirectoryShortcut _directoryShortcut;

    public ObservableDirectoryShortcut(DirectoryShortcut directoryShortcut) : base(directoryShortcut)
    {
        _directoryShortcut = directoryShortcut;
    }

    [Required(ErrorMessage = "Path is required")]
    public string Path
    {
        get => _directoryShortcut.Path;
        set => SetProperty(_directoryShortcut.Path, value, _directoryShortcut, (u, n) => u.Path = n);
    }

    public new ObservableDirectoryShortcut Clone()
    {
        return new ObservableDirectoryShortcut((DirectoryShortcut)_directoryShortcut.Clone());
    }

    public override Shortcut GetBaseShortcut()
    {
        base.GetBaseShortcut();
        return _directoryShortcut;
    }

    public override string ToString()
    {
        return _directoryShortcut.ToString();
    }


}
