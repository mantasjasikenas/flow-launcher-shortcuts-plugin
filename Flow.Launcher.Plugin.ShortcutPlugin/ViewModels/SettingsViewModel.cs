using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    private readonly ICommandsService _commandsService;

    [ObservableProperty] private string _shortcutsPath;

    [ObservableProperty] private string _variablesPath;


    public SettingsViewModel(ICommandsService commandsService, ISettingsService settingsService)
    {
        _commandsService = commandsService;
        _settingsService = settingsService;

        var settings = _settingsService.GetSettings();

        ShortcutsPath = settings.ShortcutsPath;
        VariablesPath = settings.VariablesPath;
    }

    [RelayCommand]
    private void Save()
    {
        var isModified = false;

        if (ShortcutsPath != _settingsService.GetSetting(x => x.ShortcutsPath))
        {
            _settingsService.SetSettings((x, v) => x.ShortcutsPath = v, ShortcutsPath);
            isModified = true;
        }

        if (VariablesPath != _settingsService.GetSetting(x => x.VariablesPath))
        {
            _settingsService.SetSettings((x, v) => x.VariablesPath = v, VariablesPath);
            isModified = true;
        }

        if (isModified)
        {
            _commandsService.ReloadPluginData();
        }
    }

    [RelayCommand]
    private void Reset()
    {
        _settingsService.Reset();

        ShortcutsPath = _settingsService.GetSetting(x => x.ShortcutsPath);
        VariablesPath = _settingsService.GetSetting(x => x.VariablesPath);

        _commandsService.ReloadPluginData();
    }

    [RelayCommand]
    private void Discard()
    {
        ShortcutsPath = _settingsService.GetSetting(x => x.ShortcutsPath);
        VariablesPath = _settingsService.GetSetting(x => x.VariablesPath);
    }
}