﻿using System.Runtime.CompilerServices;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
public static class ShortcutExtensions
{
    public static ObservableShortcut ToObservableShortcut(this Shortcut shortcut)
    {
        return shortcut switch
        {
            FileShortcut fileShortcut => new ObservableFileShortcut(fileShortcut),
            DirectoryShortcut directoryShortcut => new ObservableDirectoryShortcut(directoryShortcut),
            UrlShortcut urlShortcut => new ObservableUrlShortcut(urlShortcut),
            ShellShortcut shellShortcut => new ObservableShellShortcut(shellShortcut),
            GroupShortcut groupShortcut => new ObservableGroupShortcut(groupShortcut),
            _ => throw new ArgumentException("Unknown shortcut type")
        };
    }
}