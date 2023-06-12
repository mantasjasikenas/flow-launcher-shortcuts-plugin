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
    private readonly IShortcutsService _shortcutsService;
    private readonly ISettingsService _settingsService;
    private PluginInitContext _context;

    public IList<Shortcut> Shortcuts => _shortcutsService.GetShortcuts();

    public string ShortcutsPath
    {
        get => _settingsService.GetSetting(x => x.ShortcutsPath);
        set
        {
            _settingsService.SetSettings((x, v) => x.ShortcutsPath = v, value);
            _commandsService.ReloadData();
        }
    }

    public SettingsUserControl(PluginInitContext context, ISettingsService settingsService,
        IShortcutsService shortcutsService,
        ICommandsService commandsService)
    {
        _context = context;
        _commandsService = commandsService;
        _settingsService = settingsService;
        _shortcutsService = shortcutsService;


        InitializeComponent();
    }

    private void OpenConfigButton_OnClick(object sender, RoutedEventArgs e)
    {
    }
}