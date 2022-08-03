using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class ShortcutsStorage
    {
        public Dictionary<string, string> Shortcuts { get; }
        private readonly string _pluginDirectory;
        private readonly string _settingsFileName;


        public ShortcutsStorage(string pluginDirectory)
        {
            _settingsFileName = SettingsManager.GetSettingsFileName();
            _pluginDirectory = pluginDirectory;
            Shortcuts = LoadShortcutFile(pluginDirectory);
        }

        private Dictionary<string, string> LoadShortcutFile(string pluginDirectory)
        {
            var fullPath = Path.Combine(pluginDirectory, _settingsFileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    string json = File.ReadAllText(fullPath);
                    return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
                }
                catch (Exception)
                {
                    return new Dictionary<string, string>();
                }
            }

            return new Dictionary<string, string>();
        }

        private void Save()
        {
            var options = new JsonSerializerOptions() {WriteIndented = true};

            var fullPath = Path.Combine(_pluginDirectory, _settingsFileName);

            string json = JsonSerializer.Serialize(Shortcuts, options);
            File.WriteAllText(fullPath, json);
        }

        public void AddShortcut(string id, string path)
        {
            if (!Shortcuts.ContainsKey(id))
            {
                Shortcuts.Add(id, path);
                Save();
            }
            else
            {
                Shortcuts[id] = path;
                Save();
            }
        }

        public void RemoveShortcut(string id)
        {
            if (Shortcuts.ContainsKey(id))
            {
                Shortcuts.Remove(id);
                Save();
            }
        }

        public void ChangeShortcutPath(string id, string path)
        {
            if (Shortcuts.ContainsKey(id))
            {
                Shortcuts[id] = path;
                Save();
            }
        }

        public string GetSettingsFileName()
        {
            return _settingsFileName;
        }
    }
}