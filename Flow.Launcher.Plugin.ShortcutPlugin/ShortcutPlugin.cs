using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using Flow.Launcher.Plugin.ShortcutPlugin.DI;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;
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
        var args = CommandLineExtensions.SplitArguments(query.Search);
        var results = _commandsService.ResolveCommand(args);

        return results;


        /*if (query.Search == "cmd")
            return ExecuteShellShortcut(new ShellShortcut()
            {
                Command = "ping google.com",
                TargetFilePath = "powershell.exe",
                Silent = false
            });

        if (query.Search == "pl")
            return ExecutePluginShortcut(new PluginShortcut
            {
                Key = "gg",
                PluginName = "Audio Device Selector",
                RawQuery = "ad tv"
            });


        return _commandsService.TryInvokeCommand(query.Search, out var result)
            ? result
            : ResultExtensions.InitializedResult();*/
    }

    public Control CreateSettingPanel()
    {
        _settingWindow = new SettingsUserControl(_context, _settingsService, _commandsService);
        return _settingWindow;
    }

    /*private List<Result> ExecutePluginShortcut(PluginShortcut shortcut)
    {
        return ResultExtensions.SingleResult("Executing plugin shortcut", shortcut.RawQuery, Action);

        void Action()
        {
            var plugins = _context.API.GetAllPlugins();
            var plugin = plugins.First(x => x.Metadata.Name.Equals(shortcut.PluginName));

            var query = QueryBuilder.Build(shortcut.RawQuery);

            var results = plugin.Plugin.QueryAsync(query, CancellationToken.None).Result;
            results.First().Action.Invoke(null);
        }
    }

    private List<Result> ExecuteShellShortcut(ShellShortcut shortcut)
    {
        return ResultExtensions.SingleResult("Executing shell shortcut", shortcut.Command, () =>
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = shortcut.TargetFilePath,
                    Arguments = "/c " + shortcut.Command,
                    UseShellExecute = false,
                    CreateNoWindow = shortcut.Silent,
                }
            };

            process.Start();
        });
    }*/
}