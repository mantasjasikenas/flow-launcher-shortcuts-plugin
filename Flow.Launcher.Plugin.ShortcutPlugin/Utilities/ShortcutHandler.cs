using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CliWrap;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utils;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

public class ShortcutHandler : IShortcutHandler
{
    private readonly PluginInitContext _context;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IVariablesService _variablesService;

    public ShortcutHandler(IVariablesService variablesService, PluginInitContext context,
        IShortcutsRepository shortcutsRepository)
    {
        _variablesService = variablesService;
        _context = context;
        _shortcutsRepository = shortcutsRepository;
    }

    public void ExecuteShortcut(Shortcut shortcut, [CanBeNull] List<string> arguments)
    {
        var parsedArguments = arguments != null && arguments.Any()
            ? CommandLineExtensions.ParseArguments(arguments)
            : new Dictionary<string, string>();

        ExecuteShortcut(shortcut, parsedArguments);
    }

    private void ExecuteShortcut(Shortcut shortcut, IReadOnlyDictionary<string, string> parsedArguments)
    {
        switch (shortcut)
        {
            case GroupShortcut groupShortcut:
            {
                ExecuteGroupShortcut(groupShortcut, parsedArguments);
                break;
            }
            case UrlShortcut urlShortcut:
            {
                ExecuteUrlShortcut(urlShortcut, parsedArguments);
                break;
            }
            case DirectoryShortcut directoryShortcut:
            {
                ExecuteDirectoryShortcut(directoryShortcut, parsedArguments);
                break;
            }
            case FileShortcut fileShortcut:
            {
                ExecuteFileShortcut(fileShortcut, parsedArguments);
                break;
            }
            default:
            {
                _context.API.LogInfo(nameof(ShortcutHandler), "Shortcut type not supported");
                break;
            }
        }
    }

    private void ExecuteGroupShortcut(GroupShortcut groupShortcut, IReadOnlyDictionary<string, string> parsedArguments)
    {
        if (groupShortcut.Shortcuts != null)
        {
            foreach (var shortcut in groupShortcut.Shortcuts)
            {
                if (shortcut is GroupShortcut groupShortcutValue)
                {
                    if (groupShortcutValue.Keys?.Contains(groupShortcut.Key) == true)
                    {
                        _context.API.ShowMsg("Shortcut cannot contain itself.");
                        continue;
                    }
                }

                ExecuteShortcut(shortcut, parsedArguments);
            }
        }

        if (groupShortcut.Keys == null)
        {
            return;
        }

        foreach (var key in groupShortcut.Keys)
        {
            if (key.Equals(groupShortcut.Key))
            {
                _context.API.ShowMsg("Shortcut cannot contain itself.");
                continue;
            }

            var value = _shortcutsRepository.GetShortcut(key);

            if (value is GroupShortcut groupShortcutValue)
            {
                if (groupShortcutValue.Keys?.Contains(groupShortcut.Key) == true)
                {
                    _context.API.ShowMsg("Shortcut cannot contain itself.");
                    continue;
                }
            }

            if (value is not null)
            {
                ExecuteShortcut(value, parsedArguments);
            }
        }
    }

    private void ExecuteUrlShortcut(UrlShortcut urlShortcut, IReadOnlyDictionary<string, string> parsedArguments)
    {
        var path = Expand(urlShortcut.Url, parsedArguments);
        OpenUrl(path);
    }

    private void ExecuteDirectoryShortcut(DirectoryShortcut directoryShortcut,
        IReadOnlyDictionary<string, string> parsedArguments)
    {
        var path = Expand(directoryShortcut.Path, parsedArguments);
        OpenDirectory(path);
    }

    private void ExecuteFileShortcut(FileShortcut fileShortcut, IReadOnlyDictionary<string, string> parsedArguments)
    {
        var path = Expand(fileShortcut.Path, parsedArguments);
        OpenFile(path);
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
                    CreateNoWindow = shortcut.Silent
                }
            };

            process.Start();
        });
    }
}