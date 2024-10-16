using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Services;
public class ShortcutsService : IShortcutsService
{
    private const string ShortcutsPath = "C:\\Users\\tutta\\AppData\\Roaming\\FlowLauncher\\Settings\\Plugins\\Flow.Launcher.Plugin.ShortcutPlugin\\Backups\\20240925185136957\\shortcuts.json";

    private Dictionary<string, List<Shortcut>> _shortcuts = [];


    public ShortcutsService()
    {
        Task.Run(RefreshShortcutsAsync);
    }

    public async Task<IEnumerable<Shortcut>> GetShortcutsAsync()
    {
        if (_shortcuts.Count == 0)
        {
            _shortcuts = await ShortcutUtilities.ReadShortcuts(ShortcutsPath);
        }

        return _shortcuts.Values
            .SelectMany(x => x)
            .Distinct()
            .ToList();
    }

    public async Task RefreshShortcutsAsync()
    {
        _shortcuts = await ShortcutUtilities.ReadShortcuts(ShortcutsPath);
    }

    public async Task SaveShortcutAsync(Shortcut shortcut)
    {
        var key = shortcut.Key;

        if (!_shortcuts.TryGetValue(key, out var value))
        {
            value = ([]);
            _shortcuts[key] = value;
        }

        value.Add(shortcut);

        ShortcutUtilities.SaveShortcuts(_shortcuts, ShortcutsPath);
    }

    public async Task DeleteShortcutAsync(Shortcut shortcut)
    {
        var key = shortcut.Key;

        if (_shortcuts.TryGetValue(key, out var value))
        {
            value.Remove(shortcut);

            if (value.Count == 0)
            {
                _shortcuts.Remove(key);
            }
        }

        ShortcutUtilities.SaveShortcuts(_shortcuts, ShortcutsPath);
    }

    // TODO: test properly because started working magicly
    public async Task UpdateShortcutAsync(Shortcut oldShortcut, Shortcut updatedShortcut)
    {
        var oldKey = oldShortcut.Key;
        var newKey = updatedShortcut.Key;

        if (_shortcuts.TryGetValue(oldKey, out var oldKeyShortcuts))
        {
            var removed = oldKeyShortcuts.Remove(oldShortcut);

            if (!removed)
            {
                return;
            }

            if (oldKeyShortcuts.Count == 0)
            {
                _shortcuts.Remove(oldKey);
            }
        }

        if (!_shortcuts.TryGetValue(newKey, out var newKeyShortcuts))
        {
            newKeyShortcuts = [];
            _shortcuts[newKey] = newKeyShortcuts;
        }

        newKeyShortcuts.Add(updatedShortcut);

        ShortcutUtilities.SaveShortcuts(_shortcuts, ShortcutsPath);
    }
}
