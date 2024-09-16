using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class ShortcutsService : IShortcutsService
{
    private readonly IShortcutHandler _shortcutHandler;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IVariablesService _variablesService;

    public ShortcutsService(
        IShortcutsRepository shortcutsRepository,
        IShortcutHandler shortcutHandler,
        IVariablesService variablesService
    )
    {
        _shortcutsRepository = shortcutsRepository;
        _shortcutHandler = shortcutHandler;
        _variablesService = variablesService;
    }

    public List<Result> GetShortcuts(List<string> arguments)
    {
        var shortcuts = _shortcutsRepository.GetShortcuts();

        if (shortcuts.Count == 0)
        {
            return ResultExtensions.EmptyResult("No shortcuts found.");
        }

        var results = shortcuts
                      .Select(shortcut => BuildShortcutResult(
                          shortcut: shortcut,
                          arguments: arguments,
                          title: shortcut.GetTitle(),
                          subtitle: shortcut.GetSubTitle(),
                          iconPath: GetIcon(shortcut)
                      ))
                      .ToList();

        var headerResult = ResultExtensions.Result(
            "Shortcuts list",
            "Found " + shortcuts.Count + (results.Count > 1 ? " shortcuts" : " shortcut"),
            score: 100000
        );

        results.Insert(0, headerResult);

        return results;
    }

    /// <summary>
    /// Method used to get all groups in `q group list` command
    /// </summary>
    /// <returns></returns>
    public List<Result> GetGroups()
    {
        var groups = _shortcutsRepository.GetGroups();

        if (groups.Count == 0)
        {
            return ResultExtensions.EmptyResult();
        }

        return groups
               .Select(group => BuildShortcutResult(
                   shortcut: group,
                   arguments: []
               ))
               .ToList();
    }

    public List<Result> RemoveShortcut(string key)
    {
        return RemoveShortcut(_shortcutsRepository.GetPossibleShortcuts(key).ToList());
    }

    public List<Result> RemoveGroup(string key)
    {
        return RemoveShortcut(_shortcutsRepository.GetPossibleShortcuts(key)
                                                  .OfType<GroupShortcut>()
                                                  .Cast<Shortcut>()
                                                  .ToList());
    }

    private List<Result> RemoveShortcut(List<Shortcut> shortcuts)
    {
        if (shortcuts is null || shortcuts.Count == 0)
        {
            return ResultExtensions.EmptyResult("Shortcut not found", "Please provide a valid shortcut");
        }

        return shortcuts.Select(shortcut =>
                        {
                            return ResultExtensions.Result(
                                $"Remove shortcut {shortcut.GetTitle()}",
                                shortcut.ToString(),
                                titleHighlightData: [16, shortcut.GetTitle().Length],
                                action: () => { _shortcutsRepository.RemoveShortcut(shortcut); }
                            );
                        })
                        .ToList();
    }

    public List<Result> OpenShortcuts(IList<Shortcut> shortcuts, IEnumerable<string> arguments,
        bool expandGroups)
    {
        if (shortcuts is null)
        {
            return ResultExtensions.EmptyResult();
        }

        return shortcuts.SelectMany(shortcut => OpenShortcut(shortcut, arguments, expandGroups))
                        .ToList();
    }


    public IEnumerable<Result> OpenShortcut(Shortcut shortcut, IEnumerable<string> arguments, bool expandGroups)
    {
        if (shortcut is null)
        {
            return ResultExtensions.EmptyResult();
        }

        var args = arguments.ToList();
        var results = new List<Result>();

        if (expandGroups && shortcut is GroupShortcut groupShortcut)
        {
            results.AddRange(GetGroupShortcutResults(groupShortcut, args));
            return results;
        }

        var temp = $"Open {shortcut.GetDerivedType().ToLower()} ";
        var defaultKey = $"{temp}{shortcut.GetTitle()}";
        var highlightIndexes = Enumerable.Range(temp.Length, shortcut.GetTitle().Length).ToList();

        results.Add(
            BuildShortcutResult(
                shortcut,
                args,
                defaultKey,
                GetIcon(shortcut),
                titleHighlightData: highlightIndexes
            )
        );

        return results;
    }

    public List<Result> DuplicateShortcut(string existingKey, string newKey)
    {
        var existingShortcuts = _shortcutsRepository.GetShortcuts(existingKey);

        if (existingShortcuts is null || existingShortcuts.Count == 0)
        {
            return ResultExtensions.EmptyResult($"Shortcut '{existingKey}' not found");
        }

        return existingShortcuts.Select(shortcut =>
                                    ResultExtensions.Result(
                                        $"Duplicate {shortcut.GetDerivedType()} shortcut '{shortcut.GetTitle()}' to '{newKey}'",
                                        shortcut.ToString(),
                                        () => { _shortcutsRepository.DuplicateShortcut(shortcut, newKey); }))
                                .ToList();
    }

    public List<Result> ImportShortcuts()
    {
        return ResultExtensions.SingleResult(
            Resources.Import_shortcuts,
            "",
            () =>
            {
                var openFileDialog = new OpenFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Resources.Import_shortcuts,
                    CheckFileExists = true,
                    CheckPathExists = true,
                    DefaultExt = "json",
                    Filter = "JSON (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (openFileDialog.ShowDialog() != true)
                {
                    return;
                }

                _shortcutsRepository.ImportShortcuts(openFileDialog.FileName);
            }
        );
    }

    public List<Result> ExportShortcuts()
    {
        return ResultExtensions.SingleResult(
            Resources.Export_shortcuts,
            "",
            () =>
            {
                var dialog = new SaveFileDialog
                {
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    Title = Resources.Export_shortcuts,
                    FileName = "shortcuts.json",
                    CheckPathExists = true,
                    DefaultExt = "json",
                    Filter = "JSON (*.json)|*.json",
                    FilterIndex = 2,
                    RestoreDirectory = true
                };

                if (dialog.ShowDialog() != true)
                {
                    return;
                }

                var exportPath = dialog.FileName;

                _shortcutsRepository.ExportShortcuts(exportPath);
            }
        );
    }

    public void Reload()
    {
        _shortcutsRepository.ReloadShortcuts();
    }

    private static bool TryGetArguments(
        string value,
        IReadOnlyList<string> arguments,
        out Dictionary<string, string> parsedArguments
    )
    {
        parsedArguments = CommandLineExtensions.ParseArguments(value, arguments);

        if (parsedArguments.Count == 0)
        {
            return false;
        }

        foreach (var (key, _) in parsedArguments)
        {
            if (!value.Contains(key.Trim('-')))
            {
                return false;
            }
        }

        return true;
    }

    private string ExpandShortcutArguments(Shortcut shortcut, IReadOnlyList<string> arguments)
    {
        if (
            !arguments.Any()
            || !TryGetArguments(
                shortcut.ToString(),
                arguments,
                out var parsedArguments
            )
        )
        {
            return shortcut.ToString();
        }

        return _variablesService.ExpandVariables(shortcut.ToString(), parsedArguments);
    }

    private IEnumerable<Result> GetGroupShortcutResults(
        GroupShortcut groupShortcut,
        List<string> arguments
    )
    {
        var joinedArguments = string.Join(" ", arguments);

        var title = "Open group ";
        var highlightIndexes = Enumerable.Range(title.Length, groupShortcut.GetTitle().Length).ToList();
        title += groupShortcut.GetTitle();


        var results = BuildShortcutResult(
                shortcut: groupShortcut,
                arguments: arguments,
                title: title,
                subtitle: $"{groupShortcut} {joinedArguments}",
                titleHighlightData: highlightIndexes,
                score: 100
            )
            .ToList();

        if (groupShortcut.Shortcuts is not null)
        {
            results.AddRange(
                groupShortcut.Shortcuts
                             .Select(shortcut => BuildShortcutResult(
                                 shortcut: shortcut,
                                 arguments: arguments
                             ))
            );
        }

        if (groupShortcut.Keys is null)
        {
            return results;
        }

        var keyResults =
            groupShortcut.Keys
                         .Select(key =>
                             {
                                 var shortcuts = _shortcutsRepository.GetShortcuts(key);

                                 if (shortcuts is null)
                                 {
                                     return new Result
                                     {
                                         Title = "Missing shortcut.",
                                         SubTitle = $"Shortcut '{key}' not found.",
                                         IcoPath = Icons.PriorityHigh
                                     }.ToList();
                                 }

                                 return shortcuts.Select(shortcut =>
                                     BuildShortcutResult(shortcut: shortcut, arguments: arguments));
                             }
                         )
                         .SelectMany(x => x);

        results.AddRange(keyResults);

        return results;
    }

    private Result BuildShortcutResult(
        Shortcut shortcut,
        List<string> arguments,
        string title = null,
        string iconPath = null,
        IList<int> titleHighlightData = default,
        string subtitle = null,
        string autoCompleteText = null,
        int score = 0
    )
    {
        var expandedArguments = ExpandShortcutArguments(shortcut, arguments);
        var expandedAll = _variablesService.ExpandVariables(expandedArguments);

        var result = ResultExtensions.Result(
            title: (string.IsNullOrEmpty(title) ? shortcut.GetTitle() : title) + " ",
            subtitle: subtitle ?? expandedArguments,
            action: () => { _shortcutHandler.ExecuteShortcut(shortcut, arguments); },
            contextData: shortcut,
            iconPath: iconPath ?? GetIcon(shortcut),
            titleHighlightData: titleHighlightData,
            autoCompleteText: autoCompleteText ?? expandedAll,
            previewFilePath: expandedAll,
            score: score
        );

        return result;
    }

    private string GetIcon(Shortcut shortcut)
    {
        if (!string.IsNullOrEmpty(shortcut.Icon))
        {
            return shortcut.Icon;
        }

        var arguments = new Dictionary<string, string>();

        switch (shortcut)
        {
            case FileShortcut fileShortcut:
            {
                var path = _variablesService.ExpandVariables(fileShortcut.Path, arguments);

                return string.IsNullOrEmpty(fileShortcut.App)
                    ? path
                    : AppUtilities.GetApplicationPath(fileShortcut.App);
            }
            case DirectoryShortcut directoryShortcut:
            {
                return _variablesService.ExpandVariables(directoryShortcut.Path, arguments);
            }
            case UrlShortcut urlShortcut:
            {
                if (!(urlShortcut.Url.Contains("www.") || urlShortcut.Url.Contains("http") ||
                      urlShortcut.Url.Contains("https")))
                {
                    return AppUtilities.GetApplicationPath(urlShortcut.Url.Split(':')[0]);
                }

                if (string.IsNullOrEmpty(urlShortcut.App))
                {
                    return AppUtilities.GetSystemDefaultBrowser();
                }

                if (Path.Exists(urlShortcut.App))
                {
                    return urlShortcut.App;
                }

                var path = AppUtilities.GetApplicationPath(urlShortcut.App);

                return !string.IsNullOrEmpty(path) ? path : Icons.Link;
            }

            case ShellShortcut shellShortcut:
            {
                return shellShortcut.ShellType switch
                {
                    ShellType.Cmd => Icons.WindowsTerminal,
                    ShellType.Powershell => Icons.PowerShellBlack,
                    _ => Icons.Terminal
                };
            }
            case GroupShortcut:
                return Icons.TabGroup;
            default:
                return Icons.Logo;
        }
    }
}