using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class ShortcutsRepository : IShortcutsRepository
{
    private readonly string _pluginDirectory;
    private Dictionary<string, string> _shortcuts;


    public ShortcutsRepository(string pluginDirectory)
    {
        _pluginDirectory = pluginDirectory;
        _shortcuts = LoadShortcutFile();
    }

    public void ReloadShortcuts()
    {
        _shortcuts = LoadShortcutFile();
    }

    public Dictionary<string, string> GetShortcuts()
    {
        return _shortcuts;
    }

    public void AddShortcut(string id, string shortcutPath)
    {
        _shortcuts[id] = shortcutPath;
        SaveShortcutsFile();
    }

    public void RemoveShortcut(string id)
    {
        if (!_shortcuts.ContainsKey(id)) return;

        _shortcuts.Remove(id);
        SaveShortcutsFile();
    }

    public void ChangeShortcutPath(string id, string shortcutPath)
    {
        if (!_shortcuts.ContainsKey(id)) return;

        _shortcuts[id] = shortcutPath;
        SaveShortcutsFile();
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

        if (openFileDialog.ShowDialog() != true) return;
        var shortcutsPath = Path.Combine(_pluginDirectory, SettingsService.ShortcutsFileName);

        try
        {
            File.Copy(openFileDialog.FileName, shortcutsPath, true);
            ReloadShortcuts();
        }
        catch
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

    private Dictionary<string, string> LoadShortcutFile()
    {
        var fullPath = Path.Combine(_pluginDirectory, SettingsService.ShortcutsFileName);
        if (!File.Exists(fullPath)) return new Dictionary<string, string>();

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(File.ReadAllText(fullPath));
        }
        catch (Exception)
        {
            return new Dictionary<string, string>();
        }
    }

    private void SaveShortcutsFile()
    {
        var options = new JsonSerializerOptions {WriteIndented = true};
        var fullPath = Path.Combine(_pluginDirectory, SettingsService.ShortcutsFileName);

        var json = JsonSerializer.Serialize(_shortcuts, options);
        File.WriteAllText(fullPath, json);
    }
}