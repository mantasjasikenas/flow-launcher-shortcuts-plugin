using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class ShortcutsRepository : IShortcutsRepository
{
    private readonly string _pluginDirectory;
    private readonly string _shortcutsPath;
    private Dictionary<string, Shortcut> _shortcuts;


    public ShortcutsRepository(string pluginDirectory)
    {
        _pluginDirectory = pluginDirectory;
        _shortcutsPath = Path.Combine(_pluginDirectory, SettingsService.ShortcutsFileName);
        _shortcuts = ReadShortcutFile(_shortcutsPath);
    }

    public void ReloadShortcuts()
    {
        _shortcuts = ReadShortcutFile(_shortcutsPath);
    }

    public Dictionary<string, Shortcut> GetShortcuts()
    {
        return _shortcuts;
    }

    public void AddShortcut(string id, string shortcutPath)
    {
        var type = PathExtensions.ResolveShortcutType(shortcutPath);

        if (type is ShortcutType.Unknown)
        {
            MessageBox.Show(Resources.Shortcut_type_not_supported);
            return;
        }

        var shortcut = new Shortcut
        {
            Key = id,
            Path = shortcutPath,
            Type = type
        };

        _shortcuts[id] = shortcut;
        SaveShortcutsToFile();
    }

    public void RemoveShortcut(string id)
    {
        if (!_shortcuts.ContainsKey(id)) return;

        _shortcuts.Remove(id);
        SaveShortcutsToFile();
    }

    public void ChangeShortcutPath(string id, string shortcutPath)
    {
        if (!_shortcuts.ContainsKey(id)) return;

        AddShortcut(id, shortcutPath);
        SaveShortcutsToFile();
    }

    public void ImportShortcuts()
    {
        var openFileDialog = new OpenFileDialog
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = Resources.Import_shortcuts,
            CheckFileExists = true,
            CheckPathExists = true,
            DefaultExt = "json",
            Filter = "JSON (*.json)|*.json",
            FilterIndex = 2,
            RestoreDirectory = true
        };

        if (openFileDialog.ShowDialog() != true)
            return;

        try
        {
            var importedShortcuts = ReadShortcutFile(openFileDialog.FileName);

            if (importedShortcuts.Count == 0)
                throw new Exception("No shortcuts found in file");

            _shortcuts = importedShortcuts;

            SaveShortcutsToFile();
            ReloadShortcuts();
        }
        catch (Exception)
        {
            MessageBox.Show(Resources.Failed_to_import_shortcuts_file);
        }
    }

    public void ExportShortcuts()
    {
        var saveFileDialog = new SaveFileDialog
        {
            InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Title = Resources.Export_shortcuts,
            FileName = "shortcuts.json",
            CheckPathExists = true,
            DefaultExt = "json",
            Filter = "JSON (*.json)|*.json",
            FilterIndex = 2,
            RestoreDirectory = true
        };

        if (saveFileDialog.ShowDialog() != true) return;
        var shortcutsPath = Path.Combine(_pluginDirectory, SettingsService.ShortcutsFileName);

        if (!File.Exists(shortcutsPath))
        {
            MessageBox.Show(Resources.Shortcuts_file_not_found);
            return;
        }

        try
        {
            File.Copy(shortcutsPath, saveFileDialog.FileName);
        }
        catch
        {
            MessageBox.Show(Resources.Error_while_exporting_shortcuts);
        }
    }

    private Dictionary<string, Shortcut> ReadShortcutFile(string path)
    {
        if (!File.Exists(path)) return new Dictionary<string, Shortcut>();

        try
        {
            var shortcuts = JsonSerializer.Deserialize<List<Shortcut>>(File.ReadAllText(path));
            return shortcuts.ToDictionary(shortcut => shortcut.Key);
        }
        catch (Exception)
        {
            return new Dictionary<string, Shortcut>();
        }
    }

    private void SaveShortcutsToFile()
    {
        var options = new JsonSerializerOptions {WriteIndented = true};
        var fullPath = Path.Combine(_pluginDirectory, SettingsService.ShortcutsFileName);

        var json = JsonSerializer.Serialize(_shortcuts.Values, options);
        File.WriteAllText(fullPath, json);
    }
}