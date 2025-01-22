using Microsoft.Windows.ApplicationModel.Resources;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;

public static class ResourceExtensions
{
    private static readonly ResourceLoader _resourceLoader = new();

    public static string GetLocalized(this string resourceKey) => _resourceLoader.GetString(resourceKey);
}
