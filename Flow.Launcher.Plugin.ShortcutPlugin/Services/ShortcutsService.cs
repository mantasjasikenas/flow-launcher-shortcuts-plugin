using System;
using System.Collections.Generic;
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
                      .Select(shortcut =>
                      {
                          return ResultExtensions.Result(
                              shortcut.Key,
                              $"{shortcut}",
                              () => { _shortcutHandler.ExecuteShortcut(shortcut, arguments); },
                              contextData: shortcut,
                              iconPath: shortcut.GetIcon()
                          );
                      })
                      .ToList();

        var headerResult = ResultExtensions.Result(
            "Shortcuts list",
            "Found " + shortcuts.Count + (results.Count > 1 ? " shortcuts" : " shortcut"),
            score: 100000
        );

        results.Insert(0, headerResult);

        return results;
    }

    public List<Result> GetGroups()
    {
        var groups = _shortcutsRepository.GetGroups();

        if (groups.Count == 0)
        {
            return ResultExtensions.EmptyResult();
        }

        return groups
               .Select(group =>
               {
                   return ResultExtensions.Result(
                       group.Key,
                       $"{group}",
                       () => { _shortcutHandler.ExecuteShortcut(group, null); }
                   );
               })
               .ToList();
    }

    public List<Result> RemoveShortcut(string key)
    {
        return RemoveShortcut(_shortcutsRepository.GetShortcuts(key)?.ToList());
    }

    public List<Result> RemoveGroup(string key)
    {
        return RemoveShortcut(_shortcutsRepository.GetShortcuts(key)
                                                  ?
                                                  .OfType<GroupShortcut>()
                                                  .Cast<Shortcut>()
                                                  .ToList());
    }

    private List<Result> RemoveShortcut(List<Shortcut> shortcuts)
    {
        if (shortcuts is null || shortcuts.Count == 0)
        {
            return ResultExtensions.EmptyResult("Shortcut not found");
        }

        return shortcuts.Select(shortcut =>
                        {
                            return ResultExtensions.Result(
                                string.Format(Resources.ShortcutsManager_RemoveShortcut_Remove_shortcut, shortcut.Key),
                                shortcut.ToString(),
                                () => { _shortcutsRepository.RemoveShortcut(shortcut); }
                            );
                        })
                        .ToList();
    }

    public List<Result> OpenShortcuts(string key, IEnumerable<string> arguments)
    {
        var shortcuts = _shortcutsRepository.GetShortcuts(key);

        if (shortcuts is null)
        {
            return ResultExtensions.EmptyResult();
        }

        /*TODO: add group shortcut validation need to expand variables/arguments
        if (!Validators.Validators.IsValidShortcut(shortcut))
        {
            return ResultExtensions.SingleResult(
                $"Shortcut with key '{key}' is not valid.",
                "Please check the shortcut and try again.",
                titleHighlightData: Enumerable.Range(19, key.Length).ToList()
            );
        }*/

        var args = arguments.ToList();
        var results = new List<Result>();

        foreach (var shortcut in shortcuts)
        {
            if (shortcut is GroupShortcut groupShortcut)
            {
                results.AddRange(GetGroupShortcutResults(groupShortcut, args));
                continue;
            }

            var temp = $"Open {shortcut.GetDerivedType().ToLower()} ";
            var defaultKey = $"{temp}{shortcut.Key}";
            var highlightIndexes = Enumerable.Range(temp.Length, shortcut.Key.Length).ToList();

            results.Add(
                BuildResult(
                    shortcut,
                    args,
                    defaultKey,
                    shortcut.GetIcon(),
                    titleHighlightData: highlightIndexes
                )
            );
        }

        return results;
    }

    public IEnumerable<Result> OpenShortcut(Shortcut shortcut, IEnumerable<string> arguments)
    {
        if (shortcut is null)
        {
            return ResultExtensions.EmptyResult();
        }

        var args = arguments.ToList();
        var results = new List<Result>();

        // if (shortcut is GroupShortcut groupShortcut)
        // {
        //     results.AddRange(GetGroupShortcutResults(groupShortcut, args));
        //     continue;
        // }

        var temp = $"Open {shortcut.GetDerivedType().ToLower()} ";
        var defaultKey = $"{temp}{shortcut.Key}";
        var highlightIndexes = Enumerable.Range(temp.Length, shortcut.Key.Length).ToList();

        results.Add(
            BuildResult(
                shortcut,
                args,
                defaultKey,
                shortcut.GetIcon(),
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
                                        $"Duplicate {shortcut.GetDerivedType()} shortcut '{shortcut.Key}' to '{newKey}'",
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

    private string ExpandShortcut(Shortcut shortcut, IReadOnlyList<string> arguments)
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
        var highlightIndexes = Enumerable.Range(title.Length, groupShortcut.Key.Length).ToList();
        title += groupShortcut.Key;

        var results = new List<Result>
        {
            new()
            {
                Title = title,
                SubTitle = $"{groupShortcut} {joinedArguments}",
                Action = _ =>
                {
                    _shortcutHandler.ExecuteShortcut(groupShortcut, arguments);
                    return true;
                },
                IcoPath = groupShortcut.GetIcon(),
                TitleHighlightData = highlightIndexes,
                Score = 100
            }
        };

        if (groupShortcut.Shortcuts is not null)
        {
            results.AddRange(
                groupShortcut.Shortcuts.Select(shortcut =>
                {
                    var expandedShortcut = ExpandShortcut(shortcut, arguments);

                    return new Result
                    {
                        Title = $"{shortcut.Key ?? shortcut.GetDerivedType()}",
                        SubTitle = expandedShortcut,
                        Action = _ =>
                        {
                            _shortcutHandler.ExecuteShortcut(shortcut, arguments);
                            return true;
                        },
                        IcoPath = shortcut.GetIcon(),
                        ContextData = shortcut
                    };
                })
            );
        }

        if (groupShortcut.Keys is null)
        {
            return results;
        }

        var keyResults =
            groupShortcut.Keys
                         .Select(key => (groupShortcutKey: key, _shortcutsRepository.GetShortcuts(key)))
                         .Select(value =>
                             {
                                 var (key, shortcuts) = value;

                                 if (shortcuts is null)
                                 {
                                     return new List<Result>
                                     {
                                         new()
                                         {
                                             Title = "Missing shortcut.",
                                             SubTitle = $"Shortcut '{key}' not found.",
                                             IcoPath = Icons.PriorityHigh
                                         }
                                     };
                                 }

                                 var selectResults = new List<Result>();

                                 foreach (var shortcut in shortcuts)
                                 {
                                     if (shortcut is null)
                                     {
                                         selectResults.Add(new Result
                                         {
                                             Title = "Missing shortcut.",
                                             SubTitle = $"Shortcut '{key}' not found.",
                                             IcoPath = Icons.PriorityHigh
                                         });

                                         continue;
                                     }

                                     var expandedShortcut = ExpandShortcut(shortcut, arguments);

                                     results.Add(new Result
                                     {
                                         Title = $"{shortcut.Key ?? shortcut.GetDerivedType()}",
                                         SubTitle = expandedShortcut,
                                         Action = _ =>
                                         {
                                             _shortcutHandler.ExecuteShortcut(shortcut, arguments);
                                             return true;
                                         },
                                         IcoPath = shortcut.GetIcon(),
                                         ContextData = shortcut
                                     });
                                 }

                                 return selectResults;
                             }
                         )
                         .SelectMany(x => x);

        results.AddRange(keyResults);

        return results;
    }

    private Result BuildResult(
        Shortcut shortcut,
        List<string> arguments,
        string defaultKey = null,
        string iconPath = null,
        IList<int> titleHighlightData = default
    )
    {
        var expandedShortcut = ExpandShortcut(shortcut, arguments);

        return ResultExtensions.Result(
            string.IsNullOrEmpty(defaultKey) ? shortcut.GetDerivedType() : defaultKey,
            expandedShortcut,
            () => { _shortcutHandler.ExecuteShortcut(shortcut, arguments); },
            contextData: shortcut,
            iconPath: iconPath,
            titleHighlightData: titleHighlightData,
            autoCompleteText: shortcut switch
            {
                FileShortcut fileShortcut => fileShortcut.Path,
                DirectoryShortcut directoryShortcut => directoryShortcut.Path,
                UrlShortcut urlShortcut => urlShortcut.Url,
                ShellShortcut shellShortcut => shellShortcut.Arguments,
                _ => null
            }
        );
    }
}