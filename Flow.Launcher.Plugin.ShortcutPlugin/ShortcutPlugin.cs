using System.Collections.Generic;
using System.Linq;
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
    private ISettingsService _settingsService;

    private PluginInitContext _context;
    private SettingsUserControl _settingWindow;


    public void Init(PluginInitContext context)
    {
        _context = context;

        _settingsService = new SettingsService(context);
        _shortcutsRepository = new ShortcutsRepository(_settingsService);
        _shortcutsService = new ShortcutsService(_shortcutsRepository);
        _commandsService = new CommandsService(context, _shortcutsService, _settingsService);
    }

    public List<Result> Query(Query query)
    {
        if (query.Search.Equals("test"))
            return TestMethod();


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
        _settingWindow = new SettingsUserControl(_context, _settingsService, _shortcutsService, _commandsService);
        return _settingWindow;
    }

    public List<Result> TestMethod()
    {
        var plugins = _context.API.GetAllPlugins();
        /*var names = plugins.Select(x => x.Metadata.Name).ToList();
        _context.API.ShowMsg(string.Join(", ", names));*/

        // find plugin by keyword
        var plugin = plugins.FirstOrDefault(x => x.Metadata.ActionKeyword.Equals("ad"));

        return ResultExtensions.SingleResult("test", "test",
            () => { _settingsService.ModifySettings(settings => { settings.ShortcutsPath = "test"; }); });
    }
}