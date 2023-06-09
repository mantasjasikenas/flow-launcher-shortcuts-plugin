using System.Collections.Generic;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Views;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class ShortcutPlugin : IPlugin, ISettingProvider
{
    private ICommandsService _commandsService;
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
        _commandsService = new CommandsService(context, _shortcutsService);
    }

    public List<Result> Query(Query query)
    {
        // <--Query is shortcut-->
        if (_shortcutsRepository.GetShortcut(query.Search) is not null)
            return _shortcutsService.OpenShortcut(query.Search);


        // <--Query is command-->
        if (_commandsService.TryInvokeCommand(query.Search, out var commandResult))
            return commandResult;


        return ResultExtensions.InitializedResult();
    }

    public Control CreateSettingPanel()
    {
        _settingWindow = new SettingsUserControl(_context, _shortcutsService, _commandsService);
        return _settingWindow;
    }
}