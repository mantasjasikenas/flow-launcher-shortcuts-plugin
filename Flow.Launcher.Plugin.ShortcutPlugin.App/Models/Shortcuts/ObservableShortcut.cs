using System.Collections.ObjectModel;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;

public abstract class ObservableShortcut : ObservableObject
{
    private readonly Shortcut _shortcut;
    private ObservableCollection<string> _alias;

    public ObservableShortcut(Shortcut shortcut)
    {
        _shortcut = shortcut;
        _alias = new ObservableCollection<string>(shortcut.Alias ?? []);
        _alias.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Alias));
    }

    public string Key
    {
        get => _shortcut.Key;
        set
        {
            if (_shortcut.Key != value)
            {
                _shortcut.Key = value;
                OnPropertyChanged();
            }
        }
    }

    public ObservableCollection<string> Alias
    {
        get => _alias;
        set
        {
            if (_alias != value)
            {
                if (_alias != null)
                {
                    _alias.CollectionChanged -= (s, e) => OnPropertyChanged(nameof(Alias));
                }

                _alias = value;

                if (_alias != null)
                {
                    _alias.CollectionChanged += (s, e) => OnPropertyChanged(nameof(Alias));
                }

                OnPropertyChanged();
            }
        }
    }

    public string Description
    {
        get => _shortcut.Description;
        set
        {
            if (_shortcut.Description != value)
            {
                _shortcut.Description = value;
                OnPropertyChanged();
            }
        }
    }

    public string Icon
    {
        get => _shortcut.Icon;
        set
        {
            if (_shortcut.Icon != value)
            {
                _shortcut.Icon = value;
                OnPropertyChanged();
            }
        }
    }

    public string GetDerivedType() => _shortcut.GetDerivedType();

    public string GetTitle() => _shortcut.GetTitle();

    public string GetSubTitle() => _shortcut.GetSubTitle();

    public Shortcut Clone()
    {
        return (Shortcut)_shortcut.Clone();
    }
}
