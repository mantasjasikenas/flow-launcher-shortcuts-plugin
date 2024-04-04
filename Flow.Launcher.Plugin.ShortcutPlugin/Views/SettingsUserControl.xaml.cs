using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Views;

public partial class SettingsUserControl
{
    private readonly ICommandsService _commandsService;
    private readonly ISettingsService _settingsService;

    public string ShortcutsPath { get; set; }

    public string VariablesPath { get; set; }


    public SettingsUserControl(
        ISettingsService settingsService,
        ICommandsService commandsService)
    {
        _commandsService = commandsService;
        _settingsService = settingsService;

        ShortcutsPath = _settingsService.GetSetting(x => x.ShortcutsPath);
        VariablesPath = _settingsService.GetSetting(x => x.VariablesPath);

        InitializeComponent();
    }

    private void SaveButton_OnClick(object sender, RoutedEventArgs e)
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
}