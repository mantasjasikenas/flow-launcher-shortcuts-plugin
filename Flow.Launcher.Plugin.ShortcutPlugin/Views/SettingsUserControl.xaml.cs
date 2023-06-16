using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Views;

public partial class SettingsUserControl : UserControl
{
    private readonly ICommandsService _commandsService;
    private readonly ISettingsService _settingsService;
    private PluginInitContext _context;

    public string ShortcutsPath { get; set; }

    public string VariablesPath { get; set; }


    public SettingsUserControl(PluginInitContext context,
        ISettingsService settingsService,
        ICommandsService commandsService)
    {
        _context = context;
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
            _commandsService.ReloadPluginData();
    }
}