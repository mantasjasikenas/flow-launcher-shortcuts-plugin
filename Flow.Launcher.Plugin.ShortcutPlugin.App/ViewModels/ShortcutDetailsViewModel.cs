﻿using System.IO.Pipes;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.ViewModels;

public partial class ShortcutDetailsViewModel : ObservableRecipient, INavigationAware
{
    private readonly IShortcutsService _shortcutsService;
    private readonly INavigationService _navigationService;

    private Shortcut _shortcut;


    [ObservableProperty]
    public partial bool IsEditMode
    {
        get; set;
    }

    [ObservableProperty]
    public partial ObservableShortcut Shortcut
    {
        get; set;
    }

    public DetailsPageMode Mode
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

        _shortcut = args.Shortcut;
        Mode = args.Mode;
        IsEditMode = args.IsEditEnabled;

        Shortcut = ((Shortcut)_shortcut.Clone()).ToObservableShortcut();

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

    public void NavigateBack()
    {
        _navigationService.GoBack();
    }

    [RelayCommand]
    private async Task SaveButton()
    {
        if (!Shortcut.Validate())
        {
            return;
        }

        if (Mode == DetailsPageMode.New)
        {
            await _shortcutsService.SaveShortcutAsync(Shortcut.ToShortcut());
            NavigateBack();
        }
        else if (Mode == DetailsPageMode.Edit)
        {
            IsEditMode = false;
            await _shortcutsService.UpdateShortcutAsync(_shortcut, Shortcut.ToShortcut());
        }
    }

    [RelayCommand]
    private void DiscardButton()
    {
        if (Mode == DetailsPageMode.New)
        {
            NavigateBack();
        }
        else if (Mode == DetailsPageMode.Edit)
        {
            Shortcut = ((Shortcut)_shortcut.Clone()).ToObservableShortcut();
            IsEditMode = false;
        }
    }
}

public record ShorcutDetailsNavArgs(Shortcut Shortcut, DetailsPageMode Mode, bool IsEditEnabled);
