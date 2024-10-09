using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class ShortcutDetailsViewModel : ObservableRecipient, INavigationAware
{
    private Shortcut shortcut;

    public ObservableShortcut Shortcut
    {
        get;
        set;
    }

    [ObservableProperty]
    private bool isEditMode;

    public ShortcutDetailsMode Mode
    {
        get; private set;
    }

    public ShortcutDetailsViewModel()
    {

    }

    public Task OnNavigatedFrom()
    {
        return Task.CompletedTask;
    }
    public Task OnNavigatedTo(object parameter)
    {
        var args = (ShorcutDetailsNavArgs)parameter;

        shortcut = args.Shortcut;
        Mode = args.Mode;

        if (Mode == ShortcutDetailsMode.New)
        {
            IsEditMode = true;
        }

        Shortcut = shortcut.ToObservableShortcut();

        return Task.CompletedTask;
    }

    public void AddAlias(string alias)
    {
        Shortcut.Alias ??= [];
        Shortcut.Alias.Add(alias);
    }

    public void RemoveAlias(string alias)
    {
        Shortcut.Alias.Remove(alias);
    }
}

public record ShorcutDetailsNavArgs(Shortcut Shortcut, ShortcutDetailsMode Mode);
