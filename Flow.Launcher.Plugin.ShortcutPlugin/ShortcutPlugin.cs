using System.Collections.Generic;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.DI;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

// ReSharper disable once UnusedType.Global
public class ShortcutPlugin : IPlugin, ISettingProvider
{
    private ICommandsService _commandsService;
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
        _commandsService = serviceProvider.GetService<ICommandsService>();
    }


    public List<Result> Query(Query query)
    {
        return _commandsService.TryInvokeCommand(query.Search, out var result)
            ? result
            : ResultExtensions.InitializedResult();
    }

    public Control CreateSettingPanel()
    {
        _settingWindow = new SettingsUserControl(_context, _settingsService, _commandsService);
        return _settingWindow;
    }
}