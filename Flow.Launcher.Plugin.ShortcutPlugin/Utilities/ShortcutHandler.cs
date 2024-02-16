using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

public class ShortcutHandler : IShortcutHandler
{
    private readonly PluginInitContext _context;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IVariablesService _variablesService;

    public ShortcutHandler(
        IVariablesService variablesService,
        PluginInitContext context,
        IShortcutsRepository shortcutsRepository
    )
    {
        _variablesService = variablesService;
        _context = context;
        _shortcutsRepository = shortcutsRepository;
    }

    public void ExecuteShortcut(Shortcut shortcut, [CanBeNull] List<string> arguments)
    {
        var parsedArguments =
            arguments != null && arguments.Any()
                ? CommandLineExtensions.ParseArguments(shortcut.ToString(), arguments)
                : new Dictionary<string, string>();

        ExecuteShortcut(shortcut, parsedArguments);
    }

    private void ExecuteShortcut(
        Shortcut shortcut,
        IReadOnlyDictionary<string, string> parsedArguments
    )
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
            case ShellShortcut shellShortcut:
            {
                ExecuteShellShortcut(shellShortcut, parsedArguments);
                break;
            }
            default:
            {
                _context.API.LogInfo(nameof(ShortcutHandler), "Shortcut type not supported");
                break;
            }
        }
    }

    private void ExecuteGroupShortcut(
        GroupShortcut groupShortcut,
        IReadOnlyDictionary<string, string> parsedArguments
    )
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

    private void ExecuteUrlShortcut(
        UrlShortcut urlShortcut,
        IReadOnlyDictionary<string, string> parsedArguments
    )
    {
        var path = _variablesService.ExpandVariables(urlShortcut.Url, parsedArguments);
        ShortcutUtilities.OpenUrl(path, urlShortcut.App);
    }

    private void ExecuteDirectoryShortcut(
        DirectoryShortcut directoryShortcut,
        IReadOnlyDictionary<string, string> parsedArguments
    )
    {
        var path = _variablesService.ExpandVariables(directoryShortcut.Path, parsedArguments);
        ShortcutUtilities.OpenDirectory(path);
    }

    private void ExecuteFileShortcut(
        FileShortcut fileShortcut,
        IReadOnlyDictionary<string, string> parsedArguments
    )
    {
        var path = _variablesService.ExpandVariables(fileShortcut.Path, parsedArguments);
        ShortcutUtilities.OpenFile(path, fileShortcut.App);
    }

    private void ExecuteShellShortcut(
        ShellShortcut shortcut,
        IReadOnlyDictionary<string, string> parsedArguments
    )
    {
        var arguments = _variablesService.ExpandVariables(shortcut.Arguments, parsedArguments);

        switch (shortcut.ShellType)
        {
            case ShellType.Cmd:
            {
                ShortcutUtilities.OpenCmd(arguments, shortcut.Silent);
                break;
            }
            case ShellType.Powershell:
            {
                ShortcutUtilities.OpenPowershell(arguments, shortcut.Silent);
                break;
            }
            default:
            {
                _context.API.LogInfo(nameof(ShortcutHandler), "Shell type not supported");
                break;
            }
        }
    }

    // ReSharper disable once UnusedMember.Local
    [System.Obsolete]
    private List<Result> ExecutePluginShortcut(PluginInitContext context, PluginShortcut shortcut)
    {
        return ResultExtensions.SingleResult(
            "Executing plugin shortcut",
            shortcut.RawQuery,
            Action
        );

        void Action()
        {
            var plugins = context.API.GetAllPlugins();
            var plugin = plugins.First(x => x.Metadata.Name.Equals(shortcut.PluginName));

            var query = QueryBuilder.Build(shortcut.RawQuery);

            var results = plugin.Plugin.QueryAsync(query, CancellationToken.None).Result;
            results.First().Action.Invoke(null);
        }
    }
}
