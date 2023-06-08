using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Results = System.Collections.Generic.List<Flow.Launcher.Plugin.Result>;

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

    public Results Query(Query query)
    {
        var querySearch = query.Search;

        if (_shortcutsService.GetShortcuts().ContainsKey(querySearch))
            return _shortcutsService.OpenShortcut(querySearch);


        if (_settingsService.Commands.TryGetValue(querySearch, out var settingsCommand))
            return settingsCommand.Invoke();


        if (query.SearchTerms.Length < 2) return ShortcutsService.DefaultResult();
        var command = Utils.Utils.Split(querySearch);
        if (command is not null && _settingsService.Settings.TryGetValue(command.Keyword, out var setting))
            return setting.Invoke(command.Id, command.Path);


        return ShortcutsService.DefaultResult();
    }
}