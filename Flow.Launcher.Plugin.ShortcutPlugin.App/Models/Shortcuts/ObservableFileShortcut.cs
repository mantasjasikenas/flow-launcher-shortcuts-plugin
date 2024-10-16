using System.ComponentModel.DataAnnotations;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public class ObservableFileShortcut : ObservableShortcut
{
    private readonly FileShortcut _fileShortcut;

    public ObservableFileShortcut(FileShortcut fileShortcut) : base(fileShortcut)
    {
        _fileShortcut = fileShortcut;
    }

    [Required(ErrorMessage = "Path is required")]
    public string Path
    {
        get => _fileShortcut.Path;
        set => SetProperty(_fileShortcut.Path, value, _fileShortcut, (u, n) => u.Path = n);
    }

    public string App
    {
        get => _fileShortcut.App;
        set => SetProperty(_fileShortcut.App, value, _fileShortcut, (u, n) => u.App = n);
    }

    public new ObservableFileShortcut Clone()
    {
        return new ObservableFileShortcut((FileShortcut)_fileShortcut.Clone());
    }

    public override Shortcut GetBaseShortcut()
    {
        base.GetBaseShortcut();
        return _fileShortcut;
    }

    public override string ToString()
    {
        return _fileShortcut.ToString();
    }
}