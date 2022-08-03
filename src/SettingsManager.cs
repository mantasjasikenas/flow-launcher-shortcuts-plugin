using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;

namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class SettingsManager
    {
        public Dictionary<string, Func<string, string, List<Result>>> Settings { get; }
        public Dictionary<string, Func<List<Result>>> Commands { get; }
        private readonly string _pluginDirectory;
        private readonly ShortcutsManager _shortcutsManager;
        private List<Helper> _helpers;
        private const string SettingsFileName = "shortcuts.json";
        private const string HelpersFileName = "helpers.json";


        public SettingsManager(string pluginDirectory, ShortcutsManager shortcutsManager)
        {
            _shortcutsManager = shortcutsManager;
            _pluginDirectory = pluginDirectory;
            Settings = new Dictionary<string, Func<string, string, List<Result>>>(StringComparer
                .InvariantCultureIgnoreCase);
            Commands = new Dictionary<string, Func<List<Result>>>(StringComparer.InvariantCultureIgnoreCase);

            _helpers = LoadHelper();

            //Settings commands
            Settings.Add("add", _shortcutsManager.AddShortcut);
            Settings.Add("remove", _shortcutsManager.RemoveShortcut);
            Settings.Add("change", _shortcutsManager.ChangeShortcutPath);
            Settings.Add("path", _shortcutsManager.GetShortcutPath);


            //Commands
            Commands.Add("config", OpenConfig);
            Commands.Add("helpers", OpenHelpers);
            Commands.Add("reload", Reload);
            Commands.Add("list", _shortcutsManager.ListShortcuts);
            Commands.Add("help", HelpList);
        }

        public static string GetSettingsFileName()
        {
            return SettingsFileName;
        }

        public List<Result> OpenConfig()
        {
            var pathConfig = Path.Combine(_pluginDirectory,
                SettingsFileName);

            return new List<Result>
            {
                new Result()
                {
                    Title = "Open plugin config.",
                    SubTitle = $"{pathConfig}",
                    IcoPath = "images\\icon.png",

                    Action = _ =>
                    {
                        Clipboard.SetText(pathConfig);
                        new Process {StartInfo = new ProcessStartInfo(pathConfig) {UseShellExecute = true}}.Start();

                        return true;
                    }
                }
            };
        }
        
        public List<Result> OpenHelpers()
        {
            var pathConfig = Path.Combine(_pluginDirectory,
                HelpersFileName);

            return new List<Result>
            {
                new Result()
                {
                    Title = "Open plugin helpers.",
                    SubTitle = $"{pathConfig}",
                    IcoPath = "images\\icon.png",

                    Action = _ =>
                    {
                        Clipboard.SetText(pathConfig);
                        new Process {StartInfo = new ProcessStartInfo(pathConfig) {UseShellExecute = true}}.Start();

                        return true;
                    }
                }
            };
        }

        public List<Result> Reload()
        {
            return new List<Result>
            {
                new Result()
                {
                    Title = "Reload plugin.",
                    IcoPath = "images\\icon.png",
                    Action = _ =>
                    {
                        _shortcutsManager.Reload();
                        _helpers = LoadHelper();
                        return true;
                    }
                }
            };
        }

        public List<Result> HelpList()
        {
            LoadHelper();
            return Settings.Keys.Union(Commands.Keys).Select(x =>
                    new Result
                    {
                        Title = _helpers.Find(z => z.Keyword.Equals(x))?.Example,
                        SubTitle = _helpers.Find(z => z.Keyword.Equals(x))?.Description,
                        IcoPath = "images\\icon.png", Action = _ => true
                    })
                .ToList();
        }

        private List<Helper> LoadHelper()
        {
            var fullPath = Path.Combine(_pluginDirectory, HelpersFileName);

            if (File.Exists(fullPath))
            {
                try
                {
                    string json = File.ReadAllText(fullPath);
                    return JsonSerializer.Deserialize<List<Helper>>(json);
                }
                catch (Exception)
                {
                    return new List<Helper>();
                }
            }

            return new List<Helper>();
        }
    }
}