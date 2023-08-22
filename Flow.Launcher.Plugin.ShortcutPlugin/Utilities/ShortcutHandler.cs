using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utils;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

public class ShortcutHandler : IShortcutHandler
{
    private readonly IVariablesService _variablesService;
    private readonly PluginInitContext _context;

    public ShortcutHandler(IVariablesService variablesService, PluginInitContext context)
    {
        _variablesService = variablesService;
        _context = context;
    }

    public void ExecuteShortcut(Shortcut shortcut, List<string> arguments)
    {
        var parsedArguments = arguments.Any()
            ? CommandLineExtensions.ParseArguments(arguments)
            : new Dictionary<string, string>();

        switch (shortcut)
        {
            case UrlShortcut urlShortcut:
            {
                var path = Expand(urlShortcut.Url, parsedArguments);
                OpenUrl(path);

                break;
            }
            case DirectoryShortcut directoryShortcut:
            {
                var path = Expand(directoryShortcut.Path, parsedArguments);
                OpenDirectory(path);

                break;
            }
            case FileShortcut fileShortcut:
            {
                var path = Expand(fileShortcut.Path, parsedArguments);
                OpenFile(path);

                break;
            }
        }
    }

    private string Expand(string value, IReadOnlyDictionary<string, string> args)
    {
        var expandedArguments = ExpandArguments(value, args);
        return _variablesService.ExpandVariables(expandedArguments);
    }


    private static string ExpandArguments(string value, IReadOnlyDictionary<string, string> args)
    {
        foreach (var (key, arg) in args)
        {
            var trimmedKey = key.TrimStart('-');
            var replaceValue = string.Format(Constants.VariableFormat, trimmedKey);

            value = value.Replace(replaceValue, arg);
        }

        return value;
    }

    private static void OpenFile(string path)
    {
        var processStartInfo = new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true
        };
        Process.Start(processStartInfo);
    }


    private static void OpenDirectory(string path)
    {
        Cli.Wrap("explorer.exe")
           .WithArguments(path)
           .ExecuteAsync();
    }

    private static void OpenUrl(string url)
    {
        var processStartInfo = new ProcessStartInfo
        {
            UseShellExecute = true,
            FileName = url
        };
        Process.Start(processStartInfo);
    }

    // ReSharper disable once UnusedMember.Local
    private List<Result> ExecutePluginShortcut(PluginInitContext context, PluginShortcut shortcut)
    {
        return ResultExtensions.SingleResult("Executing plugin shortcut", shortcut.RawQuery, Action);

        void Action()
        {
            var plugins = context.API.GetAllPlugins();
            var plugin = plugins.First(x => x.Metadata.Name.Equals(shortcut.PluginName));

            var query = QueryBuilder.Build(shortcut.RawQuery);

            var results = plugin.Plugin.QueryAsync(query, CancellationToken.None).Result;
            results.First().Action.Invoke(null);
        }
    }

    // ReSharper disable once UnusedMember.Local
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
    }
}