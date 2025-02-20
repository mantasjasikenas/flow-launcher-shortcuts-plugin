﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IShortcutsService
{
    List<Result> RemoveShortcut(string key);
    List<Result> RemoveGroup(string key);
    List<Result> OpenShortcuts(IList<Shortcut> shortcuts, IReadOnlyDictionary<string, string> arguments, bool expandGroups);
    IEnumerable<Result> OpenShortcut(Shortcut shortcut, IReadOnlyDictionary<string, string> arguments, bool expandGroups);
    List<Result> DuplicateShortcut(string existingKey, string newKey);
    List<Result> ImportShortcuts();
    List<Result> ExportShortcuts();
    List<Result> GetShortcutsList(IReadOnlyDictionary<string, string> arguments, ShortcutType? shortcutType = null);
    List<Result> GetGroupsList();
    Task ReloadAsync();
}