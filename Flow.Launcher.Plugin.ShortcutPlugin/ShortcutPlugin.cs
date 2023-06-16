using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utils;
using Flow.Launcher.Plugin.ShortcutPlugin.Views;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

// ReSharper disable once UnusedType.Global
public class ShortcutPlugin : IPlugin, ISettingProvider
{
    private ICommandsService _commandsService;
    private IShortcutsRepository _shortcutsRepository;
    private IShortcutsService _shortcutsService;
    private ISettingsService _settingsService;
    private IHelpersRepository _helpersRepository;
    private IVariablesRepository _variablesRepository;
    private IVariablesService _variablesService;

    private PluginInitContext _context;
    private SettingsUserControl _settingWindow;


    public void Init(PluginInitContext context)
    {
        _context = context;

        _settingsService = new SettingsService(context);
        _shortcutsRepository = new ShortcutsRepository(_settingsService);
        _helpersRepository = new HelpersRepository(context);
        _shortcutsService = new ShortcutsService(_shortcutsRepository);
        _variablesRepository = new VariablesRepository(context, _settingsService);
        _variablesService = new VariablesService(_variablesRepository);
        _commandsService = new CommandsService(context, _shortcutsService, _settingsService, _helpersRepository,
            _variablesService);
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
        _settingWindow = new SettingsUserControl(_context, _settingsService, _commandsService);
        return _settingWindow;
    }

    private List<Result> TestMethod()
    {
        var plugins = _context.API.GetAllPlugins();
        var plugin = plugins.First(x => x.Metadata.ActionKeyword.Equals("ad"));

        var query = QueryBuilder.Build("ad tv");

        var action = () =>
        {
            var result = plugin.Plugin.QueryAsync(query, CancellationToken.None).Result;
            result.First().Action.Invoke(null);
        };


        return ResultExtensions.SingleResult("Dev", "", action);
    }
}