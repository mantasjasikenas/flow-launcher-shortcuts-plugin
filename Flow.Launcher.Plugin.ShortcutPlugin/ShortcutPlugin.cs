using System.Collections.Generic;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Views;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class ShortcutPlugin : IPlugin, ISettingProvider
{
    private ISettingsService _settingsService;
    private IShortcutsRepository _shortcutsRepository;
    private IShortcutsService _shortcutsService;

    private PluginInitContext _context;
    private SettingsUserControl _settingWindow;


    public void Init(PluginInitContext context)
    {
        var path = context.CurrentPluginMetadata.PluginDirectory;

        _context = context;
        _shortcutsRepository = new ShortcutsRepository(path);
        _shortcutsService = new ShortcutsService(path, _shortcutsRepository);
        _settingsService = new SettingsService(path, _shortcutsService);
    }

    public List<Result> Query(Query query)
    {
        var querySearch = query.Search;

        if (_shortcutsService.GetShortcuts().ContainsKey(querySearch))
            return _shortcutsService.OpenShortcut(querySearch);

        if (_settingsService.Commands.TryGetValue(querySearch, out var settingsCommand))
            return settingsCommand.Invoke();

        if (query.SearchTerms.Length < 2) return ResultExtensions.InitializedResult();

        var command = Command.Parse(querySearch);
        if (command is not null && _settingsService.Settings.TryGetValue(command.Keyword, out var setting))
            return setting.Invoke(command.Id, command.Path);

        return ResultExtensions.InitializedResult();
    }

    public Control CreateSettingPanel()
    {
        _settingWindow = new SettingsUserControl(_context, _shortcutsService);
        return _settingWindow;
    }
}