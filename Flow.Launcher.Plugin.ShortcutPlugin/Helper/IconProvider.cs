using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Helper;

public class IconProvider : IIconProvider
{
    private readonly IShortcutsRepository _shortcutsRepository;
    private readonly IVariablesService _variablesService;

    private static readonly Dictionary<string, string> Arguments = new();

    private List<string> _cachedIconsDirectory = [];
    private string _cachedPath;

    public IconProvider(IShortcutsRepository shortcutsRepository, IVariablesService variablesService)
    {
        _shortcutsRepository = shortcutsRepository;
        _variablesService = variablesService;
    }

    public Task InitializeAsync()
    {
        CacheIconsDirectory();

        return Task.CompletedTask;
    }

    public void Reload()
    {
        _cachedIconsDirectory.Clear();
        CacheIconsDirectory();
    }

    public string GetIcon(Shortcut shortcut)
    {
        if (!string.IsNullOrEmpty(shortcut.Icon))
        {
            if (Path.Exists(shortcut.Icon))
            {
                return shortcut.Icon;
            }

            if (TryGetIconFromIcons(out var icon, shortcut.Icon))
            {
                return icon;
            }
        }

        if (TryGetIconFromIcons(out var iconBasedOnKey, shortcut.Key))
        {
            return iconBasedOnKey;
        }

        return shortcut switch
        {
            FileShortcut fileShortcut => GetFileShortcutIcon(fileShortcut),
            DirectoryShortcut directoryShortcut => GetDirectoryShortcutIcon(directoryShortcut),
            UrlShortcut urlShortcut => GetUrlShortcutIcon(urlShortcut),
            ShellShortcut shellShortcut => GetShellShortcutIcon(shellShortcut),
            GroupShortcut => Icons.TabGroup,
            _ => Icons.Logo
        };
    }

    private string GetShellShortcutIcon(ShellShortcut shellShortcut)
    {
        return shellShortcut.ShellType switch
        {
            ShellType.Cmd => Icons.WindowsTerminal,
            ShellType.Powershell => Icons.PowerShellBlack,
            _ => Icons.Terminal
        };
    }

    private string GetUrlShortcutIcon(UrlShortcut urlShortcut)
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

    private string GetDirectoryShortcutIcon(DirectoryShortcut directoryShortcut)
    {
        return _variablesService.ExpandVariables(directoryShortcut.Path, Arguments);
    }

    private string GetFileShortcutIcon(FileShortcut fileShortcut)
    {
        var path = _variablesService.ExpandVariables(fileShortcut.Path, Arguments);

        return string.IsNullOrEmpty(fileShortcut.App)
            ? path
            : AppUtilities.GetApplicationPath(fileShortcut.App);
    }

    private bool TryGetIconFromIcons(out string icon, string key)
    {
        if (!TryGetIconsDirectoryPath(out var path))
        {
            icon = null;
            return false;
        }

        if (path != _cachedPath)
        {
            Reload();
        }

        var files = _cachedIconsDirectory;

        icon = files.FirstOrDefault(file =>
            Path.GetFileNameWithoutExtension(file)
                .Equals(key, StringComparison.OrdinalIgnoreCase) &&
            Constants.IconsExtensions.Contains(Path.GetExtension(file))
        );

        return icon != null;
    }

    private void CacheIconsDirectory()
    {
        if (!TryGetIconsDirectoryPath(out var iconsPath))
        {
            return;
        }

        _cachedPath = iconsPath;
        _cachedIconsDirectory = Directory.GetFiles(iconsPath).ToList();
    }

    private bool TryGetIconsDirectoryPath(out string path)
    {
        path = null;

        var shortcuts = _shortcutsRepository.GetShortcuts("icons");

        if (shortcuts is null || shortcuts is {Count: < 1} ||
            shortcuts.First() is not DirectoryShortcut directoryShortcut || !Directory.Exists(directoryShortcut.Path))
        {
            return false;
        }

        path = directoryShortcut.Path;

        return true;
    }
}