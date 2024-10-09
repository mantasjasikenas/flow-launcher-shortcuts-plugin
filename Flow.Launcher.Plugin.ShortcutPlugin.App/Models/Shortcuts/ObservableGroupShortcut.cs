using System.Collections.ObjectModel;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public class ObservableGroupShortcut : ObservableShortcut
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

    public bool GroupLaunch
    {
        get => _groupShortcut.GroupLaunch;
        set
        {
            if (_groupShortcut.GroupLaunch != value)
            {
                _groupShortcut.GroupLaunch = value;
                OnPropertyChanged();
            }
        }
    }

    public new ObservableGroupShortcut Clone()
    {
        return new ObservableGroupShortcut((GroupShortcut)_groupShortcut.Clone());
    }

    public override string ToString()
    {
        return _groupShortcut.ToString();
    }
}