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


    public ShortcutsService(IShortcutsRepository shortcutsRepository,
        IShortcutHandler shortcutHandler,
        PluginInitContext context
    )
    {
        _shortcutsRepository = shortcutsRepository;
        _shortcutHandler = shortcutHandler;
        _context = context;
    }

    public List<Result> GetShortcuts()
    {
        var shortcuts = _shortcutsRepository.GetShortcuts();

        if (shortcuts.Count == 0)
        {
            return ResultExtensions.EmptyResult();
        }

        return shortcuts.Select(shortcut =>
                        {
                            return ResultExtensions.Result(shortcut.Key,
                                $"{shortcut}", //  ({shortcut.GetDerivedType()})
                                () => { _shortcutHandler.ExecuteShortcut(shortcut, null); },
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

    public List<Result> OpenShortcut(string key, List<string> arguments)
    {
        var shortcut = _shortcutsRepository.GetShortcut(key);

        if (shortcut is null)
        {
            return ResultExtensions.EmptyResult();
        }

        var joinedArguments = string.Join(" ", arguments);

        if (shortcut is GroupShortcut groupShortcut)
        {
            return GetGroupShortcutResults(groupShortcut, joinedArguments);
        }

        return new List<Result>
        {
            BuildResult(shortcut, joinedArguments, $"Open {shortcut.GetDerivedType().ToLower()} shortcut",
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

    private List<Result> GetGroupShortcutResults(GroupShortcut groupShortcut, string joinedArguments)
    {
        var results = new List<Result>
        {
            new()
            {
                Title = "Open group shortcut",
                SubTitle = $"{groupShortcut} {joinedArguments}",
                Action = _ =>
                {
                    _shortcutHandler.ExecuteShortcut(groupShortcut, joinedArguments.Split(' ').ToList());
                    return true;
                },
                IcoPath = groupShortcut.GetIcon(),
                Score = 100
            }
        };


        if (groupShortcut.Shortcuts is not null)
        {
            results.AddRange(groupShortcut.Shortcuts.Select(shortcut =>
                new Result
                {
                    Title = $"{shortcut.Key ?? shortcut.GetDerivedType()}", //  ({groupShortcutValue.GetDerivedType()})
                    SubTitle = $"{shortcut} {joinedArguments}",
                    Action = _ =>
                    {
                        _shortcutHandler.ExecuteShortcut(shortcut, joinedArguments.Split(' ').ToList());
                        return true;
                    },
                    IcoPath = shortcut.GetIcon()
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
                                          if (shortcut is not null)
                                          {
                                              return new Result
                                              {
                                                  Title =
                                                      $"{shortcut.Key ?? shortcut.GetDerivedType()}", //  ({item.GetDerivedType()})
                                                  SubTitle = $"{shortcut} {joinedArguments}",
                                                  Action = _ =>
                                                  {
                                                      _shortcutHandler.ExecuteShortcut(shortcut,
                                                          joinedArguments.Split(' ').ToList());
                                                      return true;
                                                  },
                                                  IcoPath = shortcut.GetIcon()
                                              };
                                          }

                                          return new Result
                                          {
                                              Title = "Missing shortcut.",
                                              SubTitle = $"Shortcut '{key}' not found.",
                                              IcoPath = Icons.PriorityHigh
                                          };
                                      }));

        return results;
    }

    private Result BuildResult(Shortcut shortcut, string joinedArguments, string defaultKey = null,
        string iconPath = null)
    {
        return ResultExtensions.Result(
            string.IsNullOrEmpty(defaultKey) ? shortcut.GetDerivedType() : defaultKey,
            $"{shortcut} {joinedArguments}",
            () => { _shortcutHandler.ExecuteShortcut(shortcut, joinedArguments.Split(' ').ToList()); },
            iconPath: iconPath
        );
    }
}