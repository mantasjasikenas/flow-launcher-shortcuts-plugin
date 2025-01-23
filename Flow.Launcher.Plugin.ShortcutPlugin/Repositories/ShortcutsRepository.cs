﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using FuzzySharp;
using CommonShortcutUtilities = Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper.ShortcutUtilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class ShortcutsRepository : IShortcutsRepository
{
    private readonly ISettingsService _settingsService;
    private readonly IPluginManager _pluginManager;

    private Dictionary<string, List<Shortcut>> _shortcuts = [];

    public ShortcutsRepository(ISettingsService settingsService, IPluginManager pluginManager)
    {
        _settingsService = settingsService;
        _pluginManager = pluginManager;
    }

    public async Task InitializeAsync()
    {
        _shortcuts = await ReadShortcuts(_settingsService.GetSettingOrDefault(x => x.ShortcutsPath));
    }

    public IList<Shortcut>? GetShortcuts(string key)
    {
        return _shortcuts.GetValueOrDefault(key);
    }

    public bool TryGetShortcuts(string key, [MaybeNullWhen(false)] out List<Shortcut> shortcuts)
    {
        return _shortcuts.TryGetValue(key, out shortcuts);
    }

    public IList<Shortcut> GetShortcuts(ShortcutType? shortcutType = null)
    {
        return _shortcuts.Values
                         .SelectMany(x => x)
                         .Where(x => shortcutType == null || x.GetShortcutType() == shortcutType)
                         .Distinct()
                         .ToList();
    }

    public IEnumerable<Shortcut> GetPossibleShortcuts(string key)
    {
        var lowerKey = key.ToLowerInvariant();

        var shortcuts = GetShortcuts()
            .SelectMany(s => (s.Alias ?? Enumerable.Empty<string>()).Append(s.Key),
                (s, k) => new {Shortcut = s, Key = k.ToLowerInvariant()});

        return shortcuts
               .Where(x => Fuzz.PartialRatio(x.Key, lowerKey) > 90)
               .OrderByDescending(x => Fuzz.Ratio(x.Key, lowerKey))
               .Select(x => x.Shortcut)
               .Distinct()
               .ToList();
    }

    public void AddShortcut(Shortcut shortcut)
    {
        var keys = (shortcut.Alias ?? Enumerable.Empty<string>()).Append(shortcut.Key);

        foreach (var key in keys)
        {
            if (!_shortcuts.TryGetValue(key, out var value))
            {
                value = [];
                _shortcuts.Add(key, value);
            }

            value.Add(shortcut);
        }

        SaveShortcuts();
    }

    public void RemoveShortcut(Shortcut shortcut)
    {
        var keys = (shortcut.Alias ?? Enumerable.Empty<string>()).Append(shortcut.Key);

        foreach (var key in keys)
        {
            if (!_shortcuts.TryGetValue(key, out var value))
            {
                continue;
            }

            var result = value.Remove(shortcut);

            if (result && value.Count == 0)
            {
                _shortcuts.Remove(key);
            }
        }

        SaveShortcuts();
    }

    public IList<GroupShortcut> GetGroups()
    {
        return _shortcuts.Values
                         .SelectMany(x => x)
                         .OfType<GroupShortcut>()
                         .Distinct()
                         .ToList();
    }

    public void GroupShortcuts(string groupKey, bool groupLaunch, IEnumerable<string> shortcutKeys)
    {
        var group = new GroupShortcut
        {
            Key = groupKey,
            Keys = shortcutKeys.ToList(),
            GroupLaunch = groupLaunch
        };

        _shortcuts.TryGetValue(groupKey, out var value);

        if (value == null)
        {
            value = [];
            _shortcuts.Add(groupKey, value);
        }

        value.Add(group);

        SaveShortcuts();
    }

    public void DuplicateShortcut(Shortcut shortcut, string duplicateKey)
    {
        var duplicateShortcut = (Shortcut)shortcut.Clone();

        duplicateShortcut.Key = duplicateKey;

        AddShortcut(duplicateShortcut);
    }

    public async Task ReloadShortcutsAsync()
    {
        var path = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);
        _shortcuts = await ReadShortcuts(path);
    }

    public async Task ImportShortcuts(string path)
    {
        try
        {
            var shortcuts = await ReadShortcuts(path);

            if (shortcuts.Count == 0)
            {
                throw new Exception("No valid shortcuts found in the file.");
            }

            _shortcuts = shortcuts;

            SaveShortcuts();
            await ReloadShortcutsAsync();

            _pluginManager.API.ShowMsg("Shortcuts imported successfully");
        }
        catch (Exception ex)
        {
            _pluginManager.API.ShowMsg("Error while importing shortcuts");
            _pluginManager.API.LogException(nameof(ShortcutsRepository), "Error importing shortcuts", ex);
        }
    }

    public void ExportShortcuts(string path)
    {
        if (!File.Exists(_settingsService.GetSettingOrDefault(x => x.ShortcutsPath)))
        {
            _pluginManager.API.ShowMsg("No shortcuts to export");
            return;
        }

        try
        {
            File.Copy(_settingsService.GetSettingOrDefault(x => x.ShortcutsPath), path);
        }
        catch (Exception ex)
        {
            _pluginManager.API.ShowMsg("Error while exporting shortcuts");
            _pluginManager.API.LogException(nameof(ShortcutsRepository), "Error exporting shortcuts", ex);
        }
    }

    private async Task<Dictionary<string, List<Shortcut>>> ReadShortcuts(string path)
    {
        try
        {
            return await CommonShortcutUtilities.ReadShortcuts(path);
        }
        catch (Exception e)
        {
            _pluginManager.API.ShowMsg("Error while reading shortcuts. Please check the shortcuts config file.");
            _pluginManager.API.LogException(nameof(ShortcutsRepository), "Error reading shortcuts", e);

            return new Dictionary<string, List<Shortcut>>();
        }
    }

    private void SaveShortcuts()
    {
        var path = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);

        CommonShortcutUtilities.SaveShortcuts(_shortcuts, path);
    }
}