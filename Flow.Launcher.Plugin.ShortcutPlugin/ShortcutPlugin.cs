using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.DI;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.ViewModels;
using Flow.Launcher.Plugin.ShortcutPlugin.Views;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

// ReSharper disable once UnusedType.Global
public class ShortcutPlugin : IAsyncPlugin, ISettingProvider, IAsyncReloadable, IContextMenu, IAsyncInitializable
{
    internal ServiceProvider ServiceProvider
    {
        get;
        private set;
    } = null!;

    private IQueryInterpreter _queryInterpreter = null!;

    private SettingsUserControl _settingWindow = null!;
    private SettingsViewModel _settingsViewModel = null!;
    private ContextMenu _contextMenu = null!;

    private IPluginManager _pluginManager = null!;
    private IAsyncReloadable _asyncReloadable = null!;

    public async Task InitAsync(PluginInitContext context)
    {
        ServiceProvider = new ServiceCollection()
                          .ConfigureServices(context)
                          .RegisterCommands()
                          .BuildServiceProvider();


        _queryInterpreter = ServiceProvider.GetService<IQueryInterpreter>()!;
        _pluginManager = ServiceProvider.GetService<IPluginManager>()!;

        _contextMenu = ServiceProvider.GetService<ContextMenu>()!;
        _settingsViewModel = ServiceProvider.GetService<SettingsViewModel>()!;

        await InitializeAsync();

        _asyncReloadable = ServiceProvider.GetService<IAsyncReloadable>()!;
        _pluginManager.SetReloadable(_asyncReloadable);

        var ipcManagerServer = ServiceProvider.GetService<IPCManagerServer>()!;
        _ = Task.Run(() => ipcManagerServer.StartListeningAsync(CancellationToken.None));
    }


    public Task<List<Result>> QueryAsync(Query query, CancellationToken token)
    {
        _pluginManager.SetLastQuery(query);

        return Task.FromResult(_queryInterpreter.Interpret(query));
    }

    public async Task ReloadDataAsync()
    {
        await _pluginManager.ReloadDataAsync();
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
        var shortcutsRepository = ServiceProvider.GetService<IShortcutsRepository>()!;
        var variablesRepository = ServiceProvider.GetService<IVariablesRepository>()!;
        var iconProvider = ServiceProvider.GetService<IIconProvider>()!;

        Task[] tasks =
        [
            shortcutsRepository.InitializeAsync(),
            variablesRepository.InitializeAsync()
        ];

        await Task.WhenAll(tasks);
        await iconProvider.InitializeAsync();
    }
}