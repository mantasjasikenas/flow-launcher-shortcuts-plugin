using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.DI;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

// ReSharper disable once UnusedType.Global
public class ShortcutPlugin : IPlugin, ISettingProvider, IReloadable, IContextMenu, IAsyncInitializable
{
    internal ServiceProvider ServiceProvider
    {
        get; private set;
    }

    private ICommandsService _commandsService;

    private SettingsUserControl _settingWindow;
    private SettingsViewModel _settingsViewModel;
    private ContextMenu _contextMenu;

    private IPluginManager _pluginManager;
    private IReloadable _reloadable;


    public void Init(PluginInitContext context)
    {
        ServiceProvider = new ServiceCollection()
                          .ConfigureServices(context)
                          .RegisterCommands()
                          .BuildServiceProvider();


        _commandsService = ServiceProvider.GetService<ICommandsService>();
        _pluginManager = ServiceProvider.GetService<IPluginManager>();

        _contextMenu = ServiceProvider.GetService<ContextMenu>();
        _settingsViewModel = ServiceProvider.GetService<SettingsViewModel>();

        Task.Run(InitializeAsync).GetAwaiter().GetResult();

        _reloadable = ServiceProvider.GetService<IReloadable>();
        _pluginManager.SetReloadable(_reloadable);

        var ipcManagerServer = ServiceProvider.GetService<IPCManagerServer>();
        Task.Run(() => ipcManagerServer.StartListeningAsync(CancellationToken.None));
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
        _pluginManager.ReloadPluginData();
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

    public async Task InitializeAsync()
    {
        var shortcutsRepository = ServiceProvider.GetService<IShortcutsRepository>();
        var variablesRepository = ServiceProvider.GetService<IVariablesRepository>();
        var iconProvider = ServiceProvider.GetService<IIconProvider>();

        Task[] tasks =
        [
            shortcutsRepository.InitializeAsync(),
            variablesRepository.InitializeAsync()
        ];

        await Task.WhenAll(tasks);
        await iconProvider.InitializeAsync();
    }
}