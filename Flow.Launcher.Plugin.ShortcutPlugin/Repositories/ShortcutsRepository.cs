﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class ShortcutsRepository : IShortcutsRepository
{
    private readonly ISettingsService _settingsService;
    private Dictionary<string, Shortcut> _shortcuts;

    public ShortcutsRepository(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        _shortcuts = ReadShortcuts(settingsService.GetSetting(x => x.ShortcutsPath));
    }

    [CanBeNull]
    public Shortcut GetShortcut(string key)
    {
        return _shortcuts.TryGetValue(key, out var shortcut) ? shortcut : null;
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
            SaveShortcuts();
    }

    public void ReplaceShortcut(Shortcut shortcut)
    {
        if (!_shortcuts.ContainsKey(shortcut.Key)) return;

        _shortcuts[shortcut.Key] = shortcut;
        SaveShortcuts();
    }

    public void DuplicateShortcut(string existingKey, string duplicateKey)
    {
        if (!_shortcuts.ContainsKey(existingKey)) return;

        var shortcut = _shortcuts[existingKey];
        var newShortcut = (Shortcut) shortcut.Clone();

        newShortcut.Key = duplicateKey;
        _shortcuts[duplicateKey] = newShortcut;

        SaveShortcuts();
    }

    public void ReloadShortcuts()
    {
        var path = _settingsService.GetSetting(x => x.ShortcutsPath);
        _shortcuts = ReadShortcuts(path);
    }

    private static Dictionary<string, Shortcut> ReadShortcuts(string path)
    {
        if (!File.Exists(path)) return new Dictionary<string, Shortcut>();

        try
        {
            var json = File.ReadAllText(path);
            var shortcuts = JsonSerializer.Deserialize<List<Shortcut>>(json);
            return shortcuts.ToDictionary(shortcut => shortcut.Key);
        }
        catch (Exception e)
        {
            MessageBox.Show("Error while reading shortcuts file: " + e.Message);
            return new Dictionary<string, Shortcut>();
        }
    }

    private void SaveShortcuts()
    {
        var options = new JsonSerializerOptions {WriteIndented = true};
        var json = JsonSerializer.Serialize(_shortcuts.Values, options);

        var path = _settingsService.GetSetting(x => x.ShortcutsPath);
        File.WriteAllText(path, json);
    }

    public void ImportShortcuts(string path)
    {
        try
        {
            var shortcuts = ReadShortcuts(path);

            if (shortcuts.Count == 0)
                throw new Exception("No shortcuts found in file");

            _shortcuts = shortcuts;

            SaveShortcuts();
            ReloadShortcuts();
        }
        catch (Exception)
        {
            MessageBox.Show(Resources.Failed_to_import_shortcuts_file);
        }
    }

    public void ExportShortcuts(string path)
    {
        if (!File.Exists(_settingsService.GetSetting(x => x.ShortcutsPath)))
        {
            MessageBox.Show(Resources.Shortcuts_file_not_found);
            return;
        }

        try
        {
            File.Copy(_settingsService.GetSetting(x => x.ShortcutsPath), path);
        }
        catch
        {
            MessageBox.Show(Resources.Error_while_exporting_shortcuts);
        }
    }
}