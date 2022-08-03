using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class ShortcutsManager
    {
        private ShortcutsStorage _shortcutsStorage;
        private readonly string _pluginDirectory;


        public ShortcutsManager(string pluginDirectory)
        {
            _pluginDirectory = pluginDirectory;
            _shortcutsStorage = new ShortcutsStorage(pluginDirectory);
        }

        public Dictionary<string, string> GetShortcuts()
        {
            return _shortcutsStorage.Shortcuts;
        }

        public List<Result> AddShortcut(string shortcut, string path)
        {
            return new List<Result>
            {
                new Result()
                {
                    Title = $"Add shortcut '{shortcut.ToUpper()}'.",
                    SubTitle = $"{path}",
                    IcoPath = "images\\icon.png",

                    Action = _ =>
                    {
                        _shortcutsStorage.AddShortcut(shortcut, path);

                        return true;
                    }
                }
            };
        }

        public List<Result> RemoveShortcut(string shortcut, string path)
        {
            return new List<Result>
            {
                new Result()
                {
                    Title = $"Remove shortcut '{shortcut.ToUpper()}'.",
                    SubTitle = $"{path}",
                    IcoPath = "images\\icon.png",

                    Action = _ =>
                    {
                        _shortcutsStorage.RemoveShortcut(shortcut);

                        return true;
                    }
                }
            };
        }

        public List<Result> GetShortcutPath(string shortcut, string path)
        {
            if (_shortcutsStorage.Shortcuts.ContainsKey(shortcut))
                path = _shortcutsStorage.Shortcuts[shortcut];

            return new List<Result>
            {
                new Result()
                {
                    Title = $"Copy shortcut {shortcut.ToUpper()} path.",
                    SubTitle = $"{path}",
                    IcoPath = "images\\icon.png",

                    Action = _ =>
                    {
                        Clipboard.SetText(path);
                        return true;
                    }
                }
            };
        }

        public List<Result> ChangeShortcutPath(string shortcut, string path)
        {
            return new List<Result>
            {
                new Result()
                {
                    Title = $"Change shortcut '{shortcut.ToUpper()}' path.",
                    SubTitle = $"{path}",
                    IcoPath = "images\\icon.png",

                    Action = _ =>
                    {
                        _shortcutsStorage.ChangeShortcutPath(shortcut, path);

                        return true;
                    }
                }
            };
        }

        public List<Result> OpenShortcut(string shortcut)
        {
            return new List<Result>
            {
                new Result()
                {
                    Title = $"Open shortcut '{shortcut.ToUpper()}'.",
                    IcoPath = "images\\icon.png",

                    Action = _ =>
                    {
                        Utils.OpenFolder(_shortcutsStorage.Shortcuts[shortcut]);
                        return true;
                    }
                }
            };
        }

        public List<Result> PluginInitialized()
        {
            return new List<Result>
            {
                new Result
                {
                    Title = $"Plugin initialized",
                    IcoPath = "images\\icon.png",
                    Action = _ => true
                }
            };
        }

        public List<Result> ListShortcuts()
        {
            return _shortcutsStorage.Shortcuts.Select(shortcut => new Result
                {
                    Title = $"{shortcut.Key.ToUpper()}",
                    SubTitle = $"{shortcut.Value}",
                    IcoPath = "images\\icon.png",
                    Action = _ =>
                    {
                        Utils.OpenFolder(shortcut.Value);
                        return true;
                    }
                })
                .ToList();
        }

        public List<Result> Reload()
        {
            return new List<Result>
            {
                new Result
                {
                    Title = $"Reload plugin",
                    IcoPath = "images\\icon.png",
                    Action = _ =>
                    {
                        // var path = _context.CurrentPluginMetadata.PluginDirectory;
                        _shortcutsStorage = new ShortcutsStorage(_pluginDirectory);

                        return true;
                    }
                }
            };
        }
    }
}