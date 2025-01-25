using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class ShortcutsService : IShortcutsService
{
    private readonly IShortcutHandler _shortcutHandler;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IVariablesService _variablesService;
    private readonly IIconProvider _iconProvider;

    private readonly IPluginManager _pluginManager;

    public ShortcutsService(
        IShortcutsRepository shortcutsRepository,
        IShortcutHandler shortcutHandler,
        IVariablesService variablesService,
        IPluginManager pluginManager,
        IIconProvider iconProvider
    )
    {
        _shortcutsRepository = shortcutsRepository;
        _shortcutHandler = shortcutHandler;
        _variablesService = variablesService;
        _pluginManager = pluginManager;
        _iconProvider = iconProvider;
    }

    public List<Result> GetShortcutsList(IReadOnlyDictionary<string, string> arguments,
        ShortcutType? shortcutType = null)
    {
        var shortcuts = _shortcutsRepository.GetShortcuts(shortcutType);

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
                          iconPath: _iconProvider.GetIcon(shortcut),
                          action: () =>
                          {
                              _pluginManager.ChangeQueryWithAppendedKeyword(shortcut.Key);
                          },
                          hideAfterAction: false,
                          autoCompleteText: _pluginManager.AppendActionKeyword(shortcut.Key)
                      ))
                      .ToList();

        var title = shortcutType is null ? "Shortcuts" : $"{shortcutType} shortcuts";

        var headerResult = ResultExtensions.Result(
            title: title,
            subtitle: "Found " + shortcuts.Count + (results.Count > 1 ? " shortcuts" : " shortcut"),
            score: 100000,
            hideAfterAction: false
        );

        results.Insert(0, headerResult);

        return results;
    }

    /// <summary>
    /// Method used to get all groups in `q group list` command
    /// </summary>
    /// <returns></returns>
    public List<Result> GetGroupsList()
    {
        var groups = _shortcutsRepository.GetGroups();

        if (groups.Count == 0)
        {
            return ResultExtensions.EmptyResult();
        }

        var headerResult = ResultExtensions.Result(
            "Groups",
            "List of all variables",
            score: 100000
        );

        var results = groups
                      .Select(group => BuildShortcutResult(
                          shortcut: group,
                          arguments: new Dictionary<string, string>(),
                          action: () =>
                          {
                              _pluginManager.ChangeQueryWithAppendedKeyword(group.Key);
                          },
                          hideAfterAction: false,
                          autoCompleteText: _pluginManager.AppendActionKeyword(group.Key)
                      ))
                      .ToList();

        results.Insert(0, headerResult);

        return results;
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

    private List<Result> RemoveShortcut(List<Shortcut>? shortcuts)
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
                                action: () =>
                                {
                                    _shortcutsRepository.RemoveShortcut(shortcut);
                                }
                            );
                        })
                        .ToList();
    }

    public List<Result> OpenShortcuts(IList<Shortcut>? shortcuts, IReadOnlyDictionary<string, string> arguments,
        bool expandGroups)
    {
        if (shortcuts is null)
        {
            return ResultExtensions.EmptyResult();
        }

        return shortcuts.SelectMany(shortcut => OpenShortcut(shortcut, arguments, expandGroups))
                        .ToList();
    }


    public IEnumerable<Result> OpenShortcut(Shortcut shortcut, IReadOnlyDictionary<string, string> arguments,
        bool expandGroups)
    {
        var results = new List<Result>();

        if (shortcut is GroupShortcut groupShortcut)
        {
            results.AddRange(GetGroupShortcutResults(groupShortcut, expandGroups, arguments));
            return results;
        }

        var verb = shortcut is SnippetShortcut ? "Copy" : "Open";

        var prefix = $"{verb} {shortcut.GetDerivedType().ToLower()} ";
        var title = $"{prefix}{shortcut.GetTitle()}";

        var highlightIndexes = Enumerable.Range(prefix.Length, shortcut.GetTitle().Length).ToList();

        results.Add(
            BuildShortcutResult(
                shortcut,
                arguments,
                title,
                _iconProvider.GetIcon(shortcut),
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
                                        () =>
                                        {
                                            _shortcutsRepository.DuplicateShortcut(shortcut, newKey);
                                        }))
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

    public async Task ReloadAsync()
    {
        await _shortcutsRepository.ReloadShortcutsAsync();
    }

    private IEnumerable<Result> GetGroupShortcutResults(
        GroupShortcut groupShortcut,
        bool expandGroups,
        IReadOnlyDictionary<string, string> arguments
    )
    {
        var results = new List<Result>();

        // expand group when is not fully typed
        var title = "Open group ";
        var highlightIndexes = Enumerable.Range(title.Length, groupShortcut.GetTitle().Length).ToList();
        title += groupShortcut.GetTitle();

        results.Add(BuildShortcutResult(
            shortcut: groupShortcut,
            arguments: arguments,
            title: title,
            subtitle: $"{groupShortcut}",
            titleHighlightData: highlightIndexes,
            action: () =>
            {
                _pluginManager.ChangeQueryWithAppendedKeyword(groupShortcut.Key);
            },
            hideAfterAction: false,
            autoCompleteText: _pluginManager.AppendActionKeyword(groupShortcut.Key)
        ));

        if (!expandGroups)
        {
            return results;
        }

        // Fully typed shortcut key
        title = groupShortcut.GroupLaunch ? "Launch group " : "Group ";
        highlightIndexes = Enumerable.Range(title.Length, groupShortcut.GetTitle().Length).ToList();
        title += groupShortcut.GetTitle();

        results = BuildShortcutResult(
                shortcut: groupShortcut,
                arguments: arguments,
                title: title,
                subtitle: $"{groupShortcut}",
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
                                 arguments: arguments,
                                 title: string.IsNullOrWhiteSpace(shortcut.Description)
                                     ? shortcut.GetTitle()
                                     : shortcut.Description
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
                                     return ResultExtensions.SingleResult(
                                         title: title,
                                         subtitle: $"Shortcut '{key}' not found.",
                                         iconPath: Icons.PriorityHigh
                                     );
                                 }

                                 return shortcuts.Select(shortcut =>
                                     BuildShortcutResult(shortcut: shortcut,
                                         arguments: arguments,
                                         title: string.IsNullOrWhiteSpace(shortcut.Description)
                                             ? shortcut.GetTitle()
                                             : shortcut.Description
                                     )
                                 );
                             }
                         )
                         .SelectMany(x => x);

        results.AddRange(keyResults);

        return results;
    }

    private Result BuildShortcutResult(
        Shortcut shortcut,
        IReadOnlyDictionary<string, string> arguments,
        string? title = null,
        string? iconPath = null,
        IList<int>? titleHighlightData = null,
        string? subtitle = null,
        string? autoCompleteText = null,
        int score = 0,
        Action? action = null,
        bool? hideAfterAction = null
    )
    {
        var expandedArguments = _variablesService.ExpandVariables(shortcut.ToString() ?? string.Empty, arguments);
        var expandedArgumentsAndVariables = _variablesService.ExpandVariables(expandedArguments);

        var executeShortcut = shortcut is GroupShortcut {GroupLaunch: true} or not GroupShortcut;

        return ResultExtensions.Result(
            title: $"{(string.IsNullOrEmpty(title) ? shortcut.GetTitle() : title)} ",
            subtitle: subtitle ?? expandedArguments.ReplaceLineEndings(Constants.SnippetShortcutLineEnding),
            action: action ?? (executeShortcut
                ? () => _shortcutHandler.ExecuteShortcut(shortcut, arguments)
                : null),
            hideAfterAction: hideAfterAction ?? executeShortcut,
            contextData: shortcut,
            iconPath: iconPath ?? _iconProvider.GetIcon(shortcut),
            titleHighlightData: titleHighlightData,
            autoCompleteText: autoCompleteText ?? GetAutoCompletionText(shortcut, expandedArgumentsAndVariables),
            previewFilePath: expandedArgumentsAndVariables,
            score: score
        );
    }

    private string GetAutoCompletionText(Shortcut shortcut, string autoCompleteText)
    {
        var shellKeyword = _pluginManager.FindPluginActionKeyword(Constants.ShellPluginName);

        if (shortcut is ShellShortcut shellShortcut && !string.IsNullOrEmpty(shellKeyword))
        {
            return $"{shellKeyword} {shellShortcut.Arguments}";
        }

        return shortcut is GroupShortcut ? string.Empty : autoCompleteText;
    }
}