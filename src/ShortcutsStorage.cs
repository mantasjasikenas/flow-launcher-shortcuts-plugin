using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

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

    }
}