using System.Collections.Generic;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.DI;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;
using Flow.Launcher.Plugin.ShortcutPlugin.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

// ReSharper disable once UnusedType.Global
public class ShortcutPlugin : IPlugin, ISettingProvider, IReloadable, IContextMenu
{
    internal ServiceProvider ServiceProvider { get; private set; }

    private ICommandsService _commandsService;

    private SettingsUserControl _settingWindow;
    private SettingsViewModel _settingsViewModel;
    private ContextMenu _contextMenu;

    private IPluginManager _pluginManager;


    public void Init(PluginInitContext context)
    {
        var serviceProvider = new ServiceCollection()
                              .ConfigureServices(context)
                              .RegisterCommands()
                              .BuildServiceProvider();

        _commandsService = serviceProvider.GetService<ICommandsService>();
        _pluginManager = serviceProvider.GetService<IPluginManager>();
        _contextMenu = serviceProvider.GetService<ContextMenu>();
        _settingsViewModel = serviceProvider.GetService<SettingsViewModel>();

        ServiceProvider = serviceProvider;
    }

    public List<Result> Query(Query query)
    {
        _pluginManager.SetLastQuery(query);

        var args = CommandLineExtensions.SplitArguments(query.Search);
        var results = _commandsService.ResolveCommand(args, query);

        return results;
    }

    public void ReloadData()
    {
        _pluginManager.ClearLastQuery();
        _commandsService.ReloadPluginData();
    }

    public List<Result> LoadContextMenus(Result selectedResult)
    {
        return _contextMenu.LoadContextMenus(selectedResult);
    }

    public Control CreateSettingPanel()
    {
        _settingWindow = new SettingsUserControl(_settingsViewModel);

        return _settingWindow;
    }
}