using System.Reflection;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models.Shortcuts;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Test.Helpers;
using Microsoft.Extensions.DependencyInjection;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Test;

public class ShortcutPluginTest
{
    private ShortcutPlugin _plugin;
    private IShortcutsRepository _shortcutsRepository;

    private readonly string _shortcutsPath = Path.Combine(Directory.GetCurrentDirectory(), "shortcuts.json");
    private readonly string _variablesPath = Path.Combine(Directory.GetCurrentDirectory(), "variables.json");

    [OneTimeSetUp]
    public async Task Setup()
    {
        CreateEmptyFiles();
        await InitializePlugin();
        SeedShortcuts();
    }

    [OneTimeTearDown]
    public void TearDown()
    {
        File.Delete(_shortcutsPath);
        File.Delete(_variablesPath);
    }

    [Test]
    public async Task ListCommand_ShouldReturnAllShortcuts()
    {
        var query = QueryExtensions.BuildQuery("q list");
        var results = await _plugin.QueryAsync(query, CancellationToken.None);

        var shortcutsCount = _shortcutsRepository.GetShortcuts().Count;

        Assert.That(results.Count - 1, Is.EqualTo(shortcutsCount));

        await TestContext.Out.WriteLineAsync($"Shortcuts count: {shortcutsCount}");
        await TestContext.Out.WriteLineAsync($"Results count: {results.Count}");
    }

    [Test]
    [TestCase("add_dir_test")]
    public async Task AddDirectoryCommand_ShouldAddShortcut(string key)
    {
        var before = _shortcutsRepository.GetShortcuts(key)?.Count ?? 0;

        await InvokeCommand($"q add directory {key} \"C:\\\"");

        var after = _shortcutsRepository.GetShortcuts(key)?.Count ?? 0;

        Assert.That(after, Is.EqualTo(before + 1));
    }

    [Test]
    [TestCase("add_url_test")]
    public async Task AddUrlCommand_ShouldAddShortcut(string key)
    {
        var before = _shortcutsRepository.GetShortcuts(key)?.Count ?? 0;

        await InvokeCommand($"q add url {key} \"https://www.google.com\"");

        var after = _shortcutsRepository.GetShortcuts(key)?.Count ?? 0;

        Assert.That(after, Is.EqualTo(before + 1));
    }

    [Test]
    [TestCase("add_file_test")]
    public async Task AddFileCommand_ShouldAddShortcut(string key)
    {
        var before = _shortcutsRepository.GetShortcuts(key)?.Count ?? 0;

        await InvokeCommand($"q add file {key} \"C:\\test.txt\"");

        var after = _shortcutsRepository.GetShortcuts(key)?.Count ?? 0;

        Assert.That(after, Is.EqualTo(before + 1));
    }

    [Test]
    [TestCase("add_shell_cmd_test", "cmd")]
    [TestCase("add_shell_powershell_test", "powershell")]
    public async Task AddShellCommand_ShouldAddShortcut(string key, string type)
    {
        var before = _shortcutsRepository.GetShortcuts(key)?.Count ?? 0;

        await InvokeCommand($"q add shell {type} {key} true \"cmd /c echo test\"");

        var after = _shortcutsRepository.GetShortcuts(key)?.Count ?? 0;

        Assert.That(after, Is.EqualTo(before + 1));
    }


    [Test]
    public async Task RemoveCommand_ShouldRemoveShortcut()
    {
        var removeShortcut = _shortcutsRepository.GetShortcuts().First();

        await InvokeCommand("q remove " + removeShortcut.Key);

        var after = _shortcutsRepository.GetShortcuts().Contains(removeShortcut);

        Assert.That(after, Is.False);
    }

    [Test]
    public async Task DuplicateCommand_ShouldDuplicateShortcut()
    {
        var duplicateShortcut = _shortcutsRepository.GetShortcuts().First();
        const string duplicateShortcutKey = "test";

        await InvokeCommand($"q duplicate {duplicateShortcut.Key} {duplicateShortcutKey}");

        var after = _shortcutsRepository.GetShortcuts()
                                        .FirstOrDefault(x =>
                                            x.Key == duplicateShortcutKey &&
                                            x.GetType() == duplicateShortcut.GetType());

        Assert.That(after, Is.Not.Null);
    }

    private async Task InvokeCommand(string rawQuery)
    {
        var query = QueryExtensions.BuildQuery(rawQuery);

        (await _plugin.QueryAsync(query, CancellationToken.None))
            .ForEach(result => result.Action.Invoke(null));
    }

    private async Task InitializePlugin()
    {
        var pluginInitContext = new PluginInitContext(
            new PluginMetadata
            {
                ActionKeyword = "q",
                ActionKeywords = new List<string> {"q"},
                ExecuteFileName = "Flow.Launcher.Plugin.ShortcutPlugin.dll",
                IcoPath = "icon.png"
            },
            new PublicApi(_shortcutsPath, _variablesPath)
        );

        SetPluginDirectory(pluginInitContext.CurrentPluginMetadata, Directory.GetCurrentDirectory());

        _plugin = new ShortcutPlugin();
        await _plugin.InitAsync(pluginInitContext);

        _shortcutsRepository = _plugin.ServiceProvider.GetService<IShortcutsRepository>()!;
    }

    private void SetPluginDirectory(PluginMetadata pluginMetadata, string newDirectory)
    {
        var property =
            typeof(PluginMetadata).GetProperty("PluginDirectory",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
        if (property != null)
        {
            property.SetValue(pluginMetadata, newDirectory);
        }
    }

    private void CreateEmptyFiles()
    {
        File.WriteAllText(_shortcutsPath, "[]");
        File.WriteAllText(_variablesPath, "[]");
    }

    private void SeedShortcuts()
    {
        var shortcuts = new List<Shortcut>
        {
            new DirectoryShortcut
            {
                Key = "root",
                Path = "C:\\"
            },
            new UrlShortcut
            {
                Key = "google",
                Url = "https://www.google.com"
            }
        };

        shortcuts.ForEach(_shortcutsRepository.AddShortcut);
    }
}