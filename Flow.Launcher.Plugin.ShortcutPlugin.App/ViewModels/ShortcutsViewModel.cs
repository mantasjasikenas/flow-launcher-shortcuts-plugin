using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Microsoft.UI.Xaml.Controls;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class ShortcutsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IShortcutsService _shortcutsService;
    private readonly INavigationService _navigationService;

    private IEnumerable<Shortcut> Shortcuts = [];


    [ObservableProperty]
    public partial string FoundShortcutsTitle
    {
        get; set;
    }

    [ObservableProperty]
    public partial string LastUpdated
    {
        get; set;
    }


    public AutoSuggestBox AutoSuggestBox
    {
        get;
        set;
    }

    public ObservableCollection<Shortcut> FilteredShortcuts { get; private set; } = [];

    public IAsyncRelayCommand LoadShortcutsCommand
    {
        get;
    }


    public ShortcutsViewModel(IShortcutsService shortcutsService, INavigationService navigationService)
    {
        _shortcutsService = shortcutsService;
        _navigationService = navigationService;

        LoadShortcutsCommand = new AsyncRelayCommand(LoadShortcutsAsync);
    }

    public void NavigateToShortcutDetails(Shortcut shortcut, bool isEditEnabled)
    {
        _navigationService.NavigateTo(typeof(ShortcutDetailsViewModel).ToString(), new ShorcutDetailsNavArgs(shortcut, DetailsPageMode.Edit, isEditEnabled));
    }

    public async Task DeleteShortcutAsync(Shortcut shortcut)
    {
        await _shortcutsService.DeleteShortcutAsync(shortcut);
        await LoadShortcutsCommand.ExecuteAsync(null);
    }

    public void OnNewShortcutClicked(ShortcutType shortcutType)
    {
        Shortcut shortcut = shortcutType switch
        {
            ShortcutType.Directory => new DirectoryShortcut(),
            ShortcutType.File => new FileShortcut(),
            ShortcutType.Url => new UrlShortcut(),
            ShortcutType.Group => new GroupShortcut(),
            ShortcutType.Shell => new ShellShortcut(),
            _ => throw new NotImplementedException()
        };

        _navigationService.NavigateTo(typeof(ShortcutDetailsViewModel).ToString(), new ShorcutDetailsNavArgs(shortcut, DetailsPageMode.New, true));
    }

    public async Task OnNavigatedTo(object parameter)
    {
        await LoadShortcutsCommand.ExecuteAsync(null);
    }

    private void ClearSearchBox()
    {
        AutoSuggestBox.Text = string.Empty;
    }

    private async Task LoadShortcutsAsync()
    {
        ClearSearchBox();
        FilteredShortcuts.Clear();

        await _shortcutsService.RefreshShortcutsAsync();
        Shortcuts = await _shortcutsService.GetShortcutsAsync();

        LastUpdated = "Last updated: " + DateTime.Now.ToString("HH:mm:ss");

        foreach (var shortcut in Shortcuts)
        {
            FilteredShortcuts.Add(shortcut);
        }

        FoundShortcutsTitle = GenerateShortcutsTitle();
    }

    public Task OnNavigatedFrom()
    {
        return Task.CompletedTask;
    }

    public void OnFilterChanged(string filter)
    {
        var filtered = Shortcuts.Where(shortcut => Filter(shortcut, filter));

        RemoveNonMatchingShortcuts(filtered);
        AddMatchingShortcuts(filtered);

        FoundShortcutsTitle = GenerateShortcutsTitle();
    }

    private string GenerateShortcutsTitle()
    {
        var count = FilteredShortcuts.Count;

        if (count == 0)
        {
            return "We couldn't find any shortcuts";
        }

        return $"{count} shortcut{(count > 1 ? "s" : string.Empty)} found";
    }

    private static bool Filter(Shortcut shortcut, string query)
    {
        return shortcut.Key.Contains(query, StringComparison.InvariantCultureIgnoreCase) ||
            (shortcut.Alias != null && shortcut.Alias.Any(key => key.Contains(query, StringComparison.InvariantCultureIgnoreCase)));
    }

    private void RemoveNonMatchingShortcuts(IEnumerable<Shortcut> filteredData)
    {
        for (var i = FilteredShortcuts.Count - 1; i >= 0; i--)
        {
            var item = FilteredShortcuts[i];

            if (!filteredData.Contains(item))
            {
                FilteredShortcuts.Remove(item);
            }
        }
    }

    private void AddMatchingShortcuts(IEnumerable<Shortcut> filteredData)
    {
        foreach (var item in filteredData)
        {
            if (!FilteredShortcuts.Contains(item))
            {
                FilteredShortcuts.Add(item);
            }
        }
    }
}
