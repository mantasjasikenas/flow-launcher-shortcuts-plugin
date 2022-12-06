using System.Collections.Generic;
using System.Linq;

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
            var path = _context.CurrentPluginMetadata.PluginDirectory;
            _shortcuts = new ShortcutsManager(path);
            _settings = new SettingsManager(path, _shortcuts);
        }

        public List<Result> Query(Query query)
        {
            // Get command from query
            var querySearch = query.Search;


            // Shortcut command
            if (_shortcuts.GetShortcuts().ContainsKey(querySearch))
                return _shortcuts.OpenShortcut(querySearch);


            // Settings command without args
            if (_settings.Commands.ContainsKey(querySearch))
                return _settings.Commands[querySearch].Invoke();


            // Settings command with args
            if (query.SearchTerms.Length >= 2)
            {
                var command = Utils.Split(querySearch);
                if (command is not null && _settings.Settings.ContainsKey(command.Keyword))
                    return _settings.Settings[command.Keyword].Invoke(command.Id, command.Path);
            }
            


            return ShortcutsManager.Init();
        }
    }
}