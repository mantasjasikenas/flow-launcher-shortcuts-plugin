using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ISettingsService _settingsService;

    private readonly IPluginManager _pluginManager;

    [ObservableProperty] private string _shortcutsPath;

    [ObservableProperty] private string _variablesPath;


    public SettingsViewModel(IPluginManager pluginManager, ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _pluginManager = pluginManager;

        ShortcutsPath = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);
        VariablesPath = _settingsService.GetSettingOrDefault(x => x.VariablesPath);
    }

    [RelayCommand]
    private async Task Save()
    {
        var isModified = false;

        var newShortcutsPath = ShortcutsPath;
        var newVariablesPath = VariablesPath;

        var currentShortcutsPath = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);
        var currentVariablesPath = _settingsService.GetSettingOrDefault(x => x.VariablesPath);

        var defaultShortcutsPath = _settingsService.GetDefaultSetting(x => x.ShortcutsPath);
        var defaultVariablesPath = _settingsService.GetDefaultSetting(x => x.VariablesPath);

        if (newShortcutsPath != currentShortcutsPath)
        {
            if (newShortcutsPath == defaultShortcutsPath)
            {
                newShortcutsPath = string.Empty;
            }

            _settingsService.SetSettings((x, v) => x.ShortcutsPath = v, newShortcutsPath);
            isModified = true;
        }

        if (newVariablesPath != currentVariablesPath)
        {
            if (newVariablesPath == defaultVariablesPath)
            {
                newVariablesPath = string.Empty;
            }

            _settingsService.SetSettings((x, v) => x.VariablesPath = v, newVariablesPath);
            isModified = true;
        }

        if (isModified)
        {
            await _pluginManager.ReloadDataAsync();
        }
    }

    [RelayCommand]
    private async Task Reset()
    {
        _settingsService.Reset();

        ShortcutsPath = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);
        VariablesPath = _settingsService.GetSettingOrDefault(x => x.VariablesPath);

        await _pluginManager.ReloadDataAsync();
    }

    [RelayCommand]
    private void Discard()
    {
        ShortcutsPath = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);
        VariablesPath = _settingsService.GetSettingOrDefault(x => x.VariablesPath);
    }
}