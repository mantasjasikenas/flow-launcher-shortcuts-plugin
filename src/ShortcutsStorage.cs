using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Windows;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class ShortcutsStorage
    {
        public Dictionary<string, string> Shortcuts { get; private set; }
        private readonly string _pluginDirectory;


        public ShortcutsStorage(string pluginDirectory)
        {
            _pluginDirectory = pluginDirectory;
            Shortcuts = LoadShortcutFile();
        }

        public void ReloadShortcuts()
        {
            Shortcuts = LoadShortcutFile();
        }

        public void AddShortcut(string id, string shortcutPath)
        {
            if (!Shortcuts.ContainsKey(id))
                Shortcuts.Add(id, shortcutPath);
            else
                Shortcuts[id] = shortcutPath;

            SaveShortcutsFile();
        }

        public void RemoveShortcut(string id)
        {
            if (!Shortcuts.ContainsKey(id)) return;

            Shortcuts.Remove(id);
            SaveShortcutsFile();
        }

        public void ChangeShortcutPath(string id, string shortcutPath)
        {
            if (!Shortcuts.ContainsKey(id)) return;

            Shortcuts[id] = shortcutPath;
            SaveShortcutsFile();
        }

        // TODO: Check if imported file JSON is valid
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
            var shortcutsPath = Path.Combine(_pluginDirectory, SettingsManager.ShortcutsFileName);
            
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
            
            var shortcutsPath = Path.Combine(_pluginDirectory, SettingsManager.ShortcutsFileName);
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
            var fullPath = Path.Combine(_pluginDirectory, SettingsManager.ShortcutsFileName);
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
            var fullPath = Path.Combine(_pluginDirectory, SettingsManager.ShortcutsFileName);

            var json = JsonSerializer.Serialize(Shortcuts, options);
            File.WriteAllText(fullPath, json);
        }
    }
}