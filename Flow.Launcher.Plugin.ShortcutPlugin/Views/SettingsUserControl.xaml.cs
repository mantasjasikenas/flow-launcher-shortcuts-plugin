using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Views;

public partial class SettingsUserControl : UserControl
{
    //public Settings Settings { get; set; }

    private readonly IShortcutsService _shortcutsService;
    private PluginInitContext _context;

    public List<Shortcut> Shortcuts => _shortcutsService.GetShortcuts().Values.ToList();

    public SettingsUserControl(PluginInitContext context, IShortcutsService shortcutsService)
    {
        _context = context;
        _shortcutsService = shortcutsService;
        InitializeComponent();
    }

}