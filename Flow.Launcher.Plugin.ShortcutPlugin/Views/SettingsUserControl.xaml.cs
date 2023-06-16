﻿using System.Collections.Generic;
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

    public string ShortcutsPath
    {
        get => _settingsService.GetSetting(x => x.ShortcutsPath);
        set
        {
            _settingsService.SetSettings((x, v) => x.ShortcutsPath = v, value);
            _commandsService.ReloadPluginData();
        }
    }

    public string VariablesPath
    {
        get => _settingsService.GetSetting(x => x.VariablesPath);
        set
        {
            _settingsService.SetSettings((x, v) => x.VariablesPath = v, value);
            _commandsService.ReloadPluginData();
        }
    }

    public SettingsUserControl(PluginInitContext context,
        ISettingsService settingsService,
        ICommandsService commandsService)
    {
        _context = context;
        _commandsService = commandsService;
        _settingsService = settingsService;

        InitializeComponent();
    }
}