using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public partial class ObservableGroupShortcut : ObservableShortcut
{
    private readonly GroupShortcut _groupShortcut;

    public ObservableGroupShortcut(GroupShortcut groupShortcut) : base(groupShortcut)
    {
        _groupShortcut = groupShortcut;

        Shortcuts = new ObservableCollection<Shortcut>(groupShortcut.Shortcuts ?? []);
        Keys = new ObservableCollection<string>(groupShortcut.Keys ?? []);
    }

    public ObservableCollection<Shortcut> Shortcuts
    {
        get;
    }

    public ObservableCollection<string> Keys
    {
        get;
    }

    [Required(ErrorMessage = "Group launch is required")]
    public bool GroupLaunch
    {
        get => _groupShortcut.GroupLaunch;
        set => SetProperty(_groupShortcut.GroupLaunch, value, _groupShortcut, (u, n) => u.GroupLaunch = n);
    }

    public new ObservableGroupShortcut Clone()
    {
        return new ObservableGroupShortcut((GroupShortcut)_groupShortcut.Clone());
    }

    public override Shortcut GetBaseShortcut()
    {
        base.GetBaseShortcut();

        _groupShortcut.Keys = [.. Keys];
        _groupShortcut.Shortcuts = [.. Shortcuts];

        return _groupShortcut;
    }


    public override string ToString()
    {
        return _groupShortcut.ToString();
    }
}