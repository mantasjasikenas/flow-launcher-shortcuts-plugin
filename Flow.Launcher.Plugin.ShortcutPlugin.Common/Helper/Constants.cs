namespace Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;

public static class Constants
{
    public const string ShortcutsFileName = "Config\\shortcuts.json";

    public const string VariablesFileName = "Config\\variables.json";

    public const string PluginDataPath = "Settings\\Plugins\\Flow.Launcher.Plugin.ShortcutPlugin";

    public const string VariableFormat = "${{{0}}}";

    public const string BackupTimestampFormat = "yyyyMMddHHmmssfff";

    public const string BackupsFolder = "Backups";

    public const string GithubRepository =
        "https://github.com/mantasjasikenas/flow-launcher-shortcuts-plugin";

    public const string GithubIssues = $"{GithubRepository}/issues";

    public const string ReadmeUrl = GithubRepository + "/blob/master/README.md";

    public const string DiscordUsername = "mantelis130";

    public static readonly List<string> IconsExtensions =
    [
        ".png", ".jpg", ".jpeg", ".gif", ".bmp", ".tiff", ".ico"
    ];

    public const string ShortcutsPluginPipeName = "ShortcutsPluginPipeName";
}