using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Threading;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.DI;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utils;
using Flow.Launcher.Plugin.ShortcutPlugin.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

// ReSharper disable once UnusedType.Global
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


        var serviceProvider = new ServiceCollection()
                              .ConfigureServices(context)
                              .BuildServiceProvider();


        _settingsService = serviceProvider.GetService<ISettingsService>();
        _shortcutsRepository = serviceProvider.GetService<IShortcutsRepository>();
        _shortcutsService = serviceProvider.GetService<IShortcutsService>();
        _commandsService = serviceProvider.GetService<ICommandsService>();
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
        _settingWindow = new SettingsUserControl(_context, _settingsService, _commandsService);
        return _settingWindow;
    }
}