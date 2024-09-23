using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public class Reloadable : IReloadable
{
    private readonly ISettingsService _settingsService;
    private readonly IShortcutsService _shortcutsService;
    private readonly IVariablesService _variablesService;
    private readonly IIconProvider _iconProvider;

    public Reloadable(IVariablesService variablesService,
        IShortcutsService shortcutsService, ISettingsService settingsService,
        IIconProvider iconProvider
    )
    {
        _variablesService = variablesService;
        _shortcutsService = shortcutsService;
        _settingsService = settingsService;
        _iconProvider = iconProvider;
    }

    public void ReloadData()
    {
        _settingsService.Reload();
        _shortcutsService.Reload();
        _variablesService.Reload();
        _iconProvider.Reload();
    }
}