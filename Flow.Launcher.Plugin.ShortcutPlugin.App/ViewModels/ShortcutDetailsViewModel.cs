using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
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

    private readonly IShortcutsService _shortcutsService;
    private readonly INavigationService _navigationService;

    public ShortcutDetailsMode Mode
    {
        get; private set;
    }

    public ShortcutDetailsViewModel(IShortcutsService shortcutsService, INavigationService navigationService)
    {
        _shortcutsService = shortcutsService;
        _navigationService = navigationService;
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

        Shortcut = ((Shortcut)shortcut.Clone()).ToObservableShortcut();

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

    public async Task<bool> SaveNewShortcutAsync()
    {
        var shortcut = Shortcut.ToShortcut();

        if (shortcut == null || string.IsNullOrEmpty(shortcut.Key))
        {
            return false;
        }

        await _shortcutsService.SaveShortcutAsync(shortcut);

        return true;
    }

    public void NavigateBack()
    {
        _navigationService.GoBack();
    }

    public void DiscardEditedShortcut()
    {
        Shortcut = ((Shortcut)shortcut.Clone()).ToObservableShortcut();
    }

    public async Task SaveEditedShortcut()
    {
        // TODO: update doesnt work because memory reference is different
        await _shortcutsService.UpdateShortcutAsync(Shortcut.ToShortcut());
    }
}

public record ShorcutDetailsNavArgs(Shortcut Shortcut, ShortcutDetailsMode Mode);
