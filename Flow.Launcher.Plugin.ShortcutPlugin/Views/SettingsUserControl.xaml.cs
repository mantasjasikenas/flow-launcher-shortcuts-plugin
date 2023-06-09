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
    private PluginInitContext _context;

    public IList<Shortcut> Shortcuts => _shortcutsService.GetShortcuts();

    public SettingsUserControl(PluginInitContext context, IShortcutsService shortcutsService,
        ICommandsService commandsService)
    {
        _commandsService = commandsService;
        _context = context;
        _shortcutsService = shortcutsService;

        InitializeComponent();
    }

    private void OpenConfigButton_OnClick(object sender, RoutedEventArgs e)
    {
        /*var result = _commandsService.CommandsWithoutParams["config"].Invoke();
        var action = result.FirstOrDefault()?.Action;
        action?.Invoke(null);*/
    }
}