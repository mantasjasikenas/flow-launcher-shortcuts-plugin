using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class ShortcutsService : IShortcutsService
{
    // ReSharper disable once NotAccessedField.Local
    private readonly PluginInitContext _context;
    private readonly IShortcutHandler _shortcutHandler;
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IVariablesService _variablesService;


    public ShortcutsService(IShortcutsRepository shortcutsRepository,
        IShortcutHandler shortcutHandler,
        PluginInitContext context, IVariablesService variablesService)
    {
        _shortcutsRepository = shortcutsRepository;
        _shortcutHandler = shortcutHandler;
        _context = context;
        _variablesService = variablesService;
    }

    public List<Result> GetShortcuts(List<string> arguments)
    {
        var shortcuts = _shortcutsRepository.GetShortcuts();

        if (shortcuts.Count == 0)
        {
            return ResultExtensions.EmptyResult();
        }

        return shortcuts.Select(shortcut =>
                        {
                            return ResultExtensions.Result(shortcut.Key,
                                $"{shortcut}",
                                () => { _shortcutHandler.ExecuteShortcut(shortcut, arguments); },
                                contextData: shortcut,
                                iconPath: shortcut.GetIcon()
                            );
                        })
                        .ToList();
    }

    public List<Result> GetGroups()
    {
        var groups = _shortcutsRepository.GetGroups();

        if (groups.Count == 0)
        {
            return ResultExtensions.EmptyResult();
        }

        return groups.Select(group =>
                     {
                         return ResultExtensions.Result(group.Key,
                             $"{group}",
                             () => { _shortcutHandler.ExecuteShortcut(group, null); });
                     })
                     .ToList();
    }

    public List<Result> AddShortcut(Shortcut shortcut)
    {
        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_AddShortcut_Add_shortcut, shortcut.Key.ToUpper()),
            shortcut.ToString(),
            () => { _shortcutsRepository.AddShortcut(shortcut); });
    }

    public List<Result> RemoveShortcut(string key)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
        {
            return ResultExtensions.EmptyResult("Shortcut not found.");
        }

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_RemoveShortcut_Remove_shortcut, key.ToUpper()),
            shortcut.ToString(),
            () => { _shortcutsRepository.RemoveShortcut(key); });
    }

    public List<Result> GetShortcutDetails(string key)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
        {
            return ResultExtensions.EmptyResult("Shortcut not found.");
        }

        return ResultExtensions.SingleResult(
            string.Format(Resources.ShortcutsManager_GetShortcutPath_Copy_shortcut_path, key.ToUpper()),
            shortcut.ToString(),
            () =>
            {
                var details = shortcut.ToString();
                if (!string.IsNullOrEmpty(details))
                {
                    Clipboard.SetText(details);
                }
            });
    }

    public List<Result> OpenShortcut(string key, IEnumerable<string> arguments)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
        {
            return ResultExtensions.EmptyResult();
        }

        var args = arguments.ToList();

        if (shortcut is GroupShortcut groupShortcut)
        {
            return GetGroupShortcutResults(groupShortcut, args);
        }

        return new List<Result>
        {
            BuildResult(
                shortcut, 
                args,
                $"Open {shortcut.GetDerivedType().ToLower()} {shortcut.Key}",
                shortcut.GetIcon())
        };
    }

    public List<Result> DuplicateShortcut(string key, string newKey)
    {
        if (_shortcutsRepository.GetShortcut(key) is null)
        {
            return ResultExtensions.EmptyResult($"Shortcut '{key}' not found.");
        }

        return ResultExtensions.SingleResult(
            $"Duplicate shortcut '{key}' to '{newKey}'",
            "",
            () => { _shortcutsRepository.DuplicateShortcut(key, newKey); });
    }

    public List<Result> ImportShortcuts()
    {
        return ResultExtensions.SingleResult(Resources.Import_shortcuts, "",
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
            });
    }

    public List<Result> ExportShortcuts()
    {
        return ResultExtensions.SingleResult(Resources.Export_shortcuts, "",
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
            });
    }

    public void Reload()
    {
        _shortcutsRepository.ReloadShortcuts();
    }

    private bool IsArgumentsProvidedCorrectly(string value, IReadOnlyList<string> arguments,
        out Dictionary<string, string> parsedArguments)
    {
        parsedArguments = CommandLineExtensions.ParseArguments(arguments);

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
        if (!arguments.Any() || !IsArgumentsProvidedCorrectly(shortcut.ToString(), arguments,
                out var parsedArguments))
        {
            return shortcut.ToString();
        }

        return _variablesService.ExpandVariables(shortcut.ToString(), parsedArguments);
    }

    private List<Result> GetGroupShortcutResults(GroupShortcut groupShortcut, List<string> arguments)
    {
        var joinedArguments = string.Join(" ", arguments);

        var results = new List<Result>
        {
            new()
            {
                Title = "Open group shortcut",
                SubTitle = $"{groupShortcut} {joinedArguments}",
                Action = _ =>
                {
                    _shortcutHandler.ExecuteShortcut(groupShortcut, arguments);
                    return true;
                },
                IcoPath = groupShortcut.GetIcon(),
                Score = 100
            }
        };


        if (groupShortcut.Shortcuts is not null)
        {
            results.AddRange(groupShortcut.Shortcuts.Select(shortcut =>
            {
                var expandedShortcut = ExpandShortcut(shortcut, arguments);

                return new Result
                {
                    Title = $"{shortcut.Key ?? shortcut.GetDerivedType()}", //  ({groupShortcutValue.GetDerivedType()})
                    SubTitle = expandedShortcut, //$"{shortcut} {joinedArguments}",
                    Action = _ =>
                    {
                        _shortcutHandler.ExecuteShortcut(shortcut, joinedArguments.Split(' ').ToList());
                        return true;
                    },
                    IcoPath = shortcut.GetIcon(),
                    ContextData = shortcut
                };
            }));
        }

        if (groupShortcut.Keys is null)
        {
            return results;
        }

        results.AddRange(groupShortcut.Keys
                                      .Select(key => (groupShortcutKey: key,
                                          _shortcutsRepository.GetShortcut(key)))
                                      .Select(value =>
                                      {
                                          var (key, shortcut) = value;
                                          if (shortcut is null)
                                          {
                                              return new Result
                                              {
                                                  Title = "Missing shortcut.",
                                                  SubTitle = $"Shortcut '{key}' not found.",
                                                  IcoPath = Icons.PriorityHigh
                                              };
                                          }

                                          var expandedShortcut = ExpandShortcut(shortcut, arguments);

                                          return new Result
                                          {
                                              Title =
                                                  $"{shortcut.Key ?? shortcut.GetDerivedType()}", //  ({item.GetDerivedType()})
                                              SubTitle = expandedShortcut, //$"{shortcut} {joinedArguments}",
                                              Action = _ =>
                                              {
                                                  _shortcutHandler.ExecuteShortcut(shortcut,
                                                      joinedArguments.Split(' ').ToList());
                                                  return true;
                                              },
                                              IcoPath = shortcut.GetIcon(),
                                              ContextData = shortcut
                                          };
                                      }));

        return results;
    }

    private Result BuildResult(Shortcut shortcut, List<string> arguments, string defaultKey = null,
        string iconPath = null)
    {
        var expandedShortcut = ExpandShortcut(shortcut, arguments);

        return ResultExtensions.Result(
            string.IsNullOrEmpty(defaultKey) ? shortcut.GetDerivedType() : defaultKey,
            expandedShortcut, // $"{shortcut} {joinedArguments}"
            () => { _shortcutHandler.ExecuteShortcut(shortcut, arguments); },
            contextData: shortcut,
            iconPath: iconPath,
            autoCompleteText: shortcut switch
                {
                    FileShortcut fileShortcut => fileShortcut.Path,
                    DirectoryShortcut directoryShortcut => directoryShortcut.Path,
                    UrlShortcut urlShortcut => urlShortcut.Url,
                    _ => null
                }
        );
    }
}