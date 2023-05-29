using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class ShortcutPlugin : IPlugin
{
    private ISettingsService _settingsService;
    private IShortcutsRepository _shortcutsRepository;
    private IShortcutsService _shortcutsService;

    public void Init(PluginInitContext context)
    {
        var path = context.CurrentPluginMetadata.PluginDirectory;

        _shortcutsRepository = new ShortcutsRepository(path);
        _shortcutsService = new ShortcutsService(path, _shortcutsRepository);
        _settingsService = new SettingsService(path, _shortcutsService);
    }

    public List<Result> Query(Query query)
    {
        // Get command from query
        var querySearch = query.Search;

        // Shortcut command
        if (_shortcutsService.GetShortcuts().ContainsKey(querySearch))
            return _shortcutsService.OpenShortcut(querySearch);


        // Settings command without args
        if (_settingsService.Commands.TryGetValue(querySearch, out var settingsCommand))
            return settingsCommand.Invoke();


        // Settings command with args
        if (query.SearchTerms.Length < 2) return ShortcutsService.DefaultResult();
        var command = Utils.Utils.Split(querySearch);
        if (command is not null && _settingsService.Settings.TryGetValue(command.Keyword, out var setting))
            return setting.Invoke(command.Id, command.Path);


        return ShortcutsService.DefaultResult();
    }
}