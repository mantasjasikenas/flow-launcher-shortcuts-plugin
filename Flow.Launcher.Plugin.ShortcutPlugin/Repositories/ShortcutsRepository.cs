using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using FuzzySharp;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class ShortcutsRepository : IShortcutsRepository
{
    private readonly ISettingsService _settingsService;
    private readonly PluginInitContext _context;

    private Dictionary<string, List<Shortcut>> _shortcuts;

    public ShortcutsRepository(ISettingsService settingsService, PluginInitContext context)
    {
        _settingsService = settingsService;
        _context = context;

        _shortcuts = ReadShortcuts(settingsService.GetSettingOrDefault(x => x.ShortcutsPath));
    }

    public IList<Shortcut> GetShortcuts(string key)
    {
        return _shortcuts.GetValueOrDefault(key);
    }

    public IList<Shortcut> GetShortcuts()
    {
        return _shortcuts.Values
                         .SelectMany(x => x)
                         .ToList();
    }

    public IEnumerable<Shortcut> GetPossibleShortcuts(string key)
    {
        var lowerKey = key.ToLowerInvariant();

        var result = GetShortcuts()
                     .Select(s => new
                     {
                         Shortcut = s,
                         PartialScore = Fuzz.PartialRatio(s.Key.ToLowerInvariant(), lowerKey),
                         Score = Fuzz.Ratio(s.Key.ToLowerInvariant(), lowerKey)
                     })
                     .Where(x => x.PartialScore > 90)
                     .OrderByDescending(x => x.Score)
                     .Select(x => x.Shortcut)
                     .ToList();

        return result;
    }

    public void AddShortcut(Shortcut shortcut)
    {
        var result = _shortcuts.TryGetValue(shortcut.Key, out var value);

        if (!result)
        {
            value = new List<Shortcut>();
            _shortcuts.Add(shortcut.Key, value);
        }

        value.Add(shortcut);

        SaveShortcuts();
    }

    public void RemoveShortcut(Shortcut shortcut)
    {
        if (!_shortcuts.TryGetValue(shortcut.Key, out var value))
        {
            return;
        }

        var result = value.Remove(shortcut);

        if (!result)
        {
            return;
        }

        if (value.Count == 0)
        {
            _shortcuts.Remove(shortcut.Key);
        }

        SaveShortcuts();
    }

    public IList<GroupShortcut> GetGroups()
    {
        return _shortcuts.Values
                         .SelectMany(x => x)
                         .OfType<GroupShortcut>()
                         .ToList();
    }

    public void GroupShortcuts(string groupKey, IEnumerable<string> shortcutKeys)
    {
        var group = new GroupShortcut
        {
            Key = groupKey,
            Keys = shortcutKeys.ToList()
        };

        _shortcuts.TryGetValue(groupKey, out var value);

        value ??= new List<Shortcut>();
        value.Add(group);

        SaveShortcuts();
    }

    public void DuplicateShortcut(Shortcut shortcut, string duplicateKey)
    {
        var duplicateShortcut = (Shortcut) shortcut.Clone();

        duplicateShortcut.Key = duplicateKey;

        AddShortcut(duplicateShortcut);
    }

    public void ReloadShortcuts()
    {
        var path = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);

        _shortcuts = ReadShortcuts(path);
    }

    public void ImportShortcuts(string path)
    {
        try
        {
            var shortcuts = ReadShortcuts(path);

            if (shortcuts.Count == 0)
            {
                throw new Exception("No valid shortcuts found in the file.");
            }

            _shortcuts = shortcuts;

            SaveShortcuts();
            ReloadShortcuts();

            _context.API.ShowMsg("Shortcuts imported successfully");
        }
        catch (Exception ex)
        {
            _context.API.ShowMsg("Error while importing shortcuts");
            _context.API.LogException(nameof(ShortcutsRepository), "Error importing shortcuts", ex);
        }
    }

    public void ExportShortcuts(string path)
    {
        if (!File.Exists(_settingsService.GetSettingOrDefault(x => x.ShortcutsPath)))
        {
            _context.API.ShowMsg("No shortcuts to export");
            return;
        }

        try
        {
            File.Copy(_settingsService.GetSettingOrDefault(x => x.ShortcutsPath), path);
        }
        catch (Exception ex)
        {
            _context.API.ShowMsg("Error while exporting shortcuts");
            _context.API.LogException(nameof(ShortcutsRepository), "Error exporting shortcuts", ex);
        }
    }

    private Dictionary<string, List<Shortcut>> ReadShortcuts(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, List<Shortcut>>();
        }

        try
        {
            var json = File.ReadAllText(path);
            var shortcuts = JsonSerializer.Deserialize<List<Shortcut>>(json);

            return shortcuts.GroupBy(x => x.Key)
                            .ToDictionary(x => x.Key, x => x.ToList());
        }
        catch (Exception e)
        {
            _context.API.ShowMsg("Error while reading shortcuts. Please check the shortcuts config file.");
            _context.API.LogException(nameof(ShortcutsRepository), "Error reading shortcuts", e);

            return new Dictionary<string, List<Shortcut>>();
        }
    }

    private void SaveShortcuts()
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var flattenShortcuts = _shortcuts.Values
                                         .SelectMany(x => x)
                                         .ToList();

        var json = JsonSerializer.Serialize(flattenShortcuts, options);
        var path = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);
        var directory = Path.GetDirectoryName(path);

        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, json);
    }
}