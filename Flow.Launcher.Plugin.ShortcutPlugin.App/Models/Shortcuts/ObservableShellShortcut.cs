using System.ComponentModel.DataAnnotations;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public partial class ObservableShellShortcut : ObservableShortcut
{
    private readonly ShellShortcut _shellShortcut;

    public ObservableShellShortcut(ShellShortcut shellShortcut) : base(shellShortcut)
    {
        _shellShortcut = shellShortcut;
    }

    [Required(ErrorMessage = "Shell Type is required")]
    public ShellType ShellType
    {
        get => _shellShortcut.ShellType;
        set => SetProperty(_shellShortcut.ShellType, value, _shellShortcut, (u, n) => u.ShellType = n);
    }

    [Required(ErrorMessage = "Arguments is required")]
    public string Arguments
    {
        get => _shellShortcut.Arguments;
        set => SetProperty(_shellShortcut.Arguments, value, _shellShortcut, (u, n) => u.Arguments = n);
    }

    [Required(ErrorMessage = "Silent is required")]
    public bool Silent
    {
        get => _shellShortcut.Silent;
        set => SetProperty(_shellShortcut.Silent, value, _shellShortcut, (u, n) => u.Silent = n);
    }

    public new ObservableShellShortcut Clone()
    {
        return new ObservableShellShortcut((ShellShortcut)_shellShortcut.Clone());
    }

    public override Shortcut GetBaseShortcut()
    {
        base.GetBaseShortcut();
        return _shellShortcut;
    }

    public override string ToString()
    {
        return _shellShortcut.ToString();
    }
}