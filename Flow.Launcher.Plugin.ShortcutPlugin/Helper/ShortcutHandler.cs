using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public class ShortcutHandler : IShortcutHandler
{
    private readonly IPluginManager _pluginManager;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IVariablesService _variablesService;

    public ShortcutHandler(
        IVariablesService variablesService,
        IPluginManager pluginManager,
        IShortcutsRepository shortcutsRepository
    )
    {
        _variablesService = variablesService;
        _pluginManager = pluginManager;
        _shortcutsRepository = shortcutsRepository;
    }

    public void ExecuteShortcut(Shortcut shortcut, IReadOnlyDictionary<string, string> arguments)
    {
        Task.Run(() => InternalExecuteShortcut(shortcut, arguments));
    }

    private void InternalExecuteShortcut(
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
            case SnippetShortcut snippetShortcut:
            {
                ExecuteSnippetShortcut(snippetShortcut, parsedArguments);
                break;
            }
            default:
            {
                _pluginManager.API.LogInfo(nameof(ShortcutHandler), "Shortcut type not supported");
                break;
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
            case ShellType.Pwsh:
            {
                ShortcutUtilities.OpenPwsh(arguments, shortcut.Silent);
                break;
            }
            default:
            {
                _pluginManager.API.LogInfo(nameof(ShortcutHandler), "Shell type not supported");
                break;
            }
        }
    }

    private void ExecuteSnippetShortcut(
        SnippetShortcut shortcut,
        IReadOnlyDictionary<string, string> parsedArguments
    )
    {
        var value = _variablesService.ExpandVariables(shortcut.Value, parsedArguments);

        var thread = new Thread(() =>
        {
            Clipboard.SetText(value);
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    private void ExecuteGroupShortcut(GroupShortcut groupShortcut, IReadOnlyDictionary<string, string> parsedArguments)
    {
        ExecuteShortcuts(groupShortcut.Shortcuts, groupShortcut.Key, parsedArguments);
        ExecuteShortcutsByKey(groupShortcut.Keys, groupShortcut.Key, parsedArguments);
    }

    private void ExecuteShortcuts(IEnumerable<Shortcut>? shortcuts, string groupKey,
        IReadOnlyDictionary<string, string> parsedArguments)
    {
        if (shortcuts == null)
        {
            return;
        }

        var enumerable = shortcuts.ToList();

        if (enumerable.Any(shortcut => IsRecursiveGroupShortcut(shortcut, groupKey)))
        {
            _pluginManager.API.ShowMsg(Resources.Error_recursive_group_shortcut,
                "Remove the recursive group shortcut in shortcuts list of the group shortcut");
            return;
        }

        Parallel.ForEach(enumerable, shortcut =>
        {
            if (IsRecursiveGroupShortcut(shortcut, groupKey))
            {
                return;
            }

            InternalExecuteShortcut(shortcut, parsedArguments);
        });
    }

    private void ExecuteShortcutsByKey(IEnumerable<string>? keys, string groupKey,
        IReadOnlyDictionary<string, string> parsedArguments)
    {
        if (keys == null)
        {
            return;
        }

        var keysList = keys.ToList();

        if (keysList.Contains(groupKey))
        {
            _pluginManager.API.ShowMsg(Resources.Error_recursive_group_shortcut,
                "Remove the recursive group shortcut in keys list of the group shortcut");
            return;
        }

        Parallel.ForEach(keysList, key =>
        {
            if (key.Equals(groupKey))
            {
                return;
            }

            var values = _shortcutsRepository.GetShortcuts(key);

            ExecuteShortcuts(values, groupKey, parsedArguments);
        });
    }

    private static bool IsRecursiveGroupShortcut(Shortcut shortcut, string groupKey)
    {
        return shortcut is GroupShortcut groupShortcutValue && groupShortcutValue.Keys?.Contains(groupKey) == true;
    }
}