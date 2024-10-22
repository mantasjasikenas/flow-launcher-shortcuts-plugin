using System.Text;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Constants = Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers.Constants;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Services;
public class ShortcutsService : IShortcutsService
{
    private const string TestShortcutsPath = """C:\Users\tutta\AppData\Roaming\FlowLauncher\Settings\Plugins\Flow.Launcher.Plugin.ShortcutPlugin\Backups\20240925185136957\shortcuts.json""";

    private readonly ILocalSettingsService _localSettingsService;

    private Dictionary<string, List<Shortcut>> _shortcuts = [];


    public ShortcutsService(ILocalSettingsService localSettingsService)
    {
        Task.Run(RefreshShortcutsAsync);
        _localSettingsService = localSettingsService;
    }

    private async Task<Dictionary<string, List<Shortcut>>> ReadShortcuts()
    {
        var path = await GetShortcutsPath();

        if (string.IsNullOrEmpty(path))
        {
            return [];
        }

        return await ShortcutUtilities.ReadShortcuts(path);
    }

    private async Task SaveShortcuts(Dictionary<string, List<Shortcut>> shortcuts)
    {
        var path = await GetShortcutsPath();

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        ShortcutUtilities.SaveShortcuts(shortcuts, path);
    }

    private async Task<string> GetShortcutsPath()
    {
        return await _localSettingsService.ReadSettingAsync<string>(Constants.ShortcutPathKey) ?? TestShortcutsPath;
    }


    public async Task<IEnumerable<Shortcut>> GetShortcutsAsync()
    {
        if (_shortcuts.Count == 0)
        {
            _shortcuts = await ReadShortcuts();
        }

        return _shortcuts.Values
            .SelectMany(x => x)
            .Distinct()
            .ToList();
    }

    public async Task RefreshShortcutsAsync()
    {
        _shortcuts = await ReadShortcuts();
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

        await SaveShortcuts(_shortcuts);
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

        await SaveShortcuts(_shortcuts);
    }

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

        await SaveShortcuts(_shortcuts);
    }
}
