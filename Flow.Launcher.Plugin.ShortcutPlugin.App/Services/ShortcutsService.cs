using System.Text;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Services;
public class ShortcutsService : IShortcutsService
{
    private Dictionary<string, List<Shortcut>> _shortcuts = [];
    private readonly IPCManagerClient _iPCManagerClient;

    public ShortcutsService(IPCManagerClient iPCManagerClient)
    {
        Task.Run(RefreshShortcutsAsync);
        this._iPCManagerClient = iPCManagerClient;
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

        _ = _iPCManagerClient.SendMessageAsync(IPCCommand.ReloadPluginData.ToString(), CancellationToken.None);
    }


    private async Task<string> GetShortcutsPath()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var shortcutsPluginPath = Directory.GetParent(appDirectory)?.Parent?.FullName;

        return ShortcutUtilities.GetShortcutsPath(shortcutsPluginPath);
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
