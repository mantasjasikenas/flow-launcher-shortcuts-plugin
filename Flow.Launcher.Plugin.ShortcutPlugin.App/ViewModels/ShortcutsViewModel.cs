using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class ShortcutsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IShortcutsService _shortcutsService;

    [ObservableProperty]
    private Shortcut? selected;

    public ObservableCollection<Shortcut> Shortcuts { get; private set; } = new ObservableCollection<Shortcut>();

    public ShortcutsViewModel(IShortcutsService shortcutsService)
    {
        _shortcutsService = shortcutsService;
    }

    public async void OnNavigatedTo(object parameter)
    {
        Shortcuts.Clear();

        var data = await _shortcutsService.GetShortcutsAsync();

        foreach (var item in data)
        {
            Shortcuts.Add(item);
        }
    }

    public void OnNavigatedFrom()
    {
    }

    public void EnsureItemSelected()
    {
        Selected ??= Shortcuts.FirstOrDefault();
    }
}
