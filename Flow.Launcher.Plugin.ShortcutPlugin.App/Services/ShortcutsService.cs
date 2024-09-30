using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Services;
public class ShortcutsService : IShortcutsService
{
    private const string ShortcutsPath = "C:\\Users\\tutta\\AppData\\Roaming\\FlowLauncher\\Settings\\Plugins\\Flow.Launcher.Plugin.ShortcutPlugin\\Backups\\20240925185136957\\shortcuts.json";

    public async Task<IEnumerable<Shortcut>> GetShortcutsAsync()
    {
        var shortcuts = await ShortcutUtilities.ReadShortcuts(ShortcutsPath);

        return shortcuts.Values
            .SelectMany(x => x)
            .Distinct()
            .ToList();
    }
}
