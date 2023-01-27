using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Manager;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class ShortcutPlugin : IPlugin
{
    private ShortcutsManager _shortcutsManager;
    private SettingsManager _settingsManager;
    private PluginInitContext _context;


    public void Init(PluginInitContext context)
    {
        _context = context;
        var path = _context.CurrentPluginMetadata.PluginDirectory;
        _shortcutsManager = new ShortcutsManager(path);
        _settingsManager = new SettingsManager(path, _shortcutsManager);
    }

    public List<Result> Query(Query query)
    {
        // Get command from query
        var querySearch = query.Search;

        // Shortcut command
        if (_shortcutsManager.GetShortcuts().ContainsKey(querySearch))
            return _shortcutsManager.OpenShortcut(querySearch);


        // Settings command without args
        if (_settingsManager.Commands.ContainsKey(querySearch))
            return _settingsManager.Commands[querySearch].Invoke();


        // Settings command with args
        if (query.SearchTerms.Length < 2) return ShortcutsManager.Init();
        var command = Utils.Utils.Split(querySearch);
        if (command is not null && _settingsManager.Settings.ContainsKey(command.Keyword))
            return _settingsManager.Settings[command.Keyword].Invoke(command.Id, command.Path);


        return ShortcutsManager.Init();
    }
}