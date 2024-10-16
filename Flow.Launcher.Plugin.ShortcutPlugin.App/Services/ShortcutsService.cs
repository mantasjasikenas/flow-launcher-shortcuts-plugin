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

    public async Task SaveShortcutAsync(Shortcut shortcut)
    {
        var shortcuts = await ShortcutUtilities.ReadShortcuts(ShortcutsPath);

        var key = shortcut.Key;

        if (!shortcuts.TryGetValue(key, out var value))
        {
            value = ([]);
            shortcuts[key] = value;
        }

        value.Add(shortcut);

        ShortcutUtilities.SaveShortcuts(shortcuts, ShortcutsPath);
    }

    public async Task DeleteShortcutAsync(Shortcut shortcut)
    {
        var shortcuts = await ShortcutUtilities.ReadShortcuts(ShortcutsPath);

        var key = shortcut.Key;

        if (shortcuts.TryGetValue(key, out var value))
        {
            value.Remove(shortcut);
        }

        ShortcutUtilities.SaveShortcuts(shortcuts, ShortcutsPath);
    }

    // TODO not working
    public async Task UpdateShortcutAsync(Shortcut shortcut)
    {
        var shortcuts = await ShortcutUtilities.ReadShortcuts(ShortcutsPath);

        var key = shortcut.Key;

        if (shortcuts.TryGetValue(key, out var value))
        {
            // TODO: This is a temporary fix to update the shortcut
            var existingShortcut = value.FirstOrDefault(x => x.Key == shortcut.Key && x.GetDerivedType() == shortcut.GetDerivedType());
            
            if (existingShortcut != null)
            {
                value.Remove(existingShortcut);
                value.Add(shortcut);
            }
        }
        else
        {
            shortcuts[key] = [shortcut];
        }

        ShortcutUtilities.SaveShortcuts(shortcuts, ShortcutsPath);
    }
}
