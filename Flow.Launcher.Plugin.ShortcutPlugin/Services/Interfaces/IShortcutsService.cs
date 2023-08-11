﻿using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IShortcutsService
{
    List<Result> AddShortcut(string key, string path, ShortcutType type);
    List<Result> RemoveShortcut(string key);
    List<Result> GetShortcutDetails(string key);
    List<Result> OpenShortcut(string key);
    List<Result> DuplicateShortcut(string key, string newKey);
    List<Result> ImportShortcuts();
    List<Result> ExportShortcuts();
    List<Result> GetShortcuts();
    void Reload();
}