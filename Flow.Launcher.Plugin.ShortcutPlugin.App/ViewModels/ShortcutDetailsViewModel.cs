using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class ShortcutDetailsViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private Shortcut shortcut;

    [ObservableProperty]
    private bool isEditMode;

    public ShortcutDetailsViewModel()
    {

    }

    public Task OnNavigatedFrom()
    {
        return Task.CompletedTask;
    }
    public Task OnNavigatedTo(object parameter)
    {
        Shortcut = (Shortcut)parameter;

        return Task.CompletedTask;
    }
}
