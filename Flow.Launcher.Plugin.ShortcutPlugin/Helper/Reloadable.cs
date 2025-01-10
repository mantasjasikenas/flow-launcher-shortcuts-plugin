using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public class Reloadable : IAsyncReloadable
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

    public async Task ReloadDataAsync()
    {
        _settingsService.Reload();

        await Task.WhenAll(
            Task.Run(() => _variablesService.ReloadAsync()),
            Task.Run(() => _shortcutsService.ReloadAsync())
        );

        _iconProvider.Reload();
    }
}