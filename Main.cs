using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin
{
    public class ShortcutPlugin : IPlugin
    {
        private ShortcutsManager _shortcuts;
        private SettingsManager _settings;
        private PluginInitContext _context;


        public void Init(PluginInitContext context)
        {
            _context = context;
            string path = _context.CurrentPluginMetadata.PluginDirectory;
            _shortcuts = new ShortcutsManager(path);
            _settings = new SettingsManager(path, _shortcuts);
        }

        public List<Result> Query(Query query)
        {
            // Get command
            var search = query.Search;


            // Shortcut command
            if (_shortcuts.GetShortcuts().ContainsKey(search))
                return _shortcuts.OpenShortcut(search);


            // Settings command without args
            if (_settings.Commands.ContainsKey(search))
                return _settings.Commands[search].Invoke();


            // Settings command with args
            var command = Utils.Split(search);
            if (command != null)
            {
                if (_settings.Settings.ContainsKey(command.Keyword))
                    return _settings.Settings[command.Keyword].Invoke(command.Id, command.Path);
            }


            return _shortcuts.PluginInitialized();
        }
    }
}