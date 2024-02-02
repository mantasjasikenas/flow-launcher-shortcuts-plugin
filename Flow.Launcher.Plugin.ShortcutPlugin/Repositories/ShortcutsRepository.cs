using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class ShortcutsRepository : IShortcutsRepository
{
    private readonly ISettingsService _settingsService;
    private readonly PluginInitContext _context;
    private Dictionary<string, Shortcut> _shortcuts;

    public ShortcutsRepository(ISettingsService settingsService, PluginInitContext context)
    {
        _settingsService = settingsService;
        _context = context;
        _shortcuts = ReadShortcuts(settingsService.GetSetting(x => x.ShortcutsPath));
    }

    public Shortcut GetShortcut(string key)
    {
        return _shortcuts.GetValueOrDefault(key);
    }

    public IList<Shortcut> GetShortcuts()
    {
        return _shortcuts.Values.ToList();
    }

    public void AddShortcut(Shortcut shortcut)
    {
        _shortcuts[shortcut.Key] = shortcut;
        SaveShortcuts();
    }

    public void RemoveShortcut(string key)
    {
        var result = _shortcuts.Remove(key);

        if (result)
        {
            SaveShortcuts();
        }
    }

    public List<GroupShortcut> GetGroups()
    {
        return _shortcuts.Values.OfType<GroupShortcut>().ToList();
    }

    public void GroupShortcuts(string groupKey, IEnumerable<string> shortcutKeys)
    {
        var group = new GroupShortcut
        {
            Key = groupKey,
            Keys = shortcutKeys.ToList()
        };

        _shortcuts[groupKey] = group;
        SaveShortcuts();
    }

    public void ReplaceShortcut(Shortcut shortcut)
    {
        if (!_shortcuts.ContainsKey(shortcut.Key))
        {
            return;
        }

        _shortcuts[shortcut.Key] = shortcut;

        SaveShortcuts();
    }

    public void DuplicateShortcut(string existingKey, string duplicateKey)
    {
        if (!_shortcuts.TryGetValue(existingKey, out var value))
        {
            return;
        }

        var newShortcut = (Shortcut)value.Clone();

        newShortcut.Key = duplicateKey;
        _shortcuts[duplicateKey] = newShortcut;

        SaveShortcuts();
    }

    public void ReloadShortcuts()
    {
        var path = _settingsService.GetSetting(x => x.ShortcutsPath);
        _shortcuts = ReadShortcuts(path);
    }

    public void ImportShortcuts(string path)
    {
        try
        {
            var shortcuts = ReadShortcuts(path);

            if (shortcuts.Count == 0)
            {
                throw new Exception();
            }

            _shortcuts = shortcuts;

            SaveShortcuts();
            ReloadShortcuts();
        }
        catch (Exception ex)
        {
            _context.API.ShowMsg("Error while importing shortcuts");
            _context.API.LogException(nameof(ShortcutsRepository), "Error importing shortcuts", ex);
        }
    }

    public void ExportShortcuts(string path)
    {
        if (!File.Exists(_settingsService.GetSetting(x => x.ShortcutsPath)))
        {
            _context.API.ShowMsg("No shortcuts to export");
            return;
        }

        try
        {
            File.Copy(_settingsService.GetSetting(x => x.ShortcutsPath), path);
        }
        catch (Exception ex)
        {
            _context.API.ShowMsg("Error while exporting shortcuts");
            _context.API.LogException(nameof(ShortcutsRepository), "Error exporting shortcuts", ex);
        }
    }

    private Dictionary<string, Shortcut> ReadShortcuts(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, Shortcut>();
        }

        try
        {
            var json = File.ReadAllText(path);
            var shortcuts = JsonSerializer.Deserialize<List<Shortcut>>(json);

            return shortcuts.ToDictionary(shortcut => shortcut.Key);
        }
        catch (Exception e)
        {
            _context.API.ShowMsg("Error while reading shortcuts");
            _context.API.LogException(nameof(ShortcutsRepository), "Error reading shortcuts", e);

            return new Dictionary<string, Shortcut>();
        }
    }

    private void SaveShortcuts()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(_shortcuts.Values, options);
        var path = _settingsService.GetSetting(x => x.ShortcutsPath);
        var directory = Path.GetDirectoryName(path);

        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, json);
    }
}