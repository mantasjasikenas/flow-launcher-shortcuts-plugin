using Flow.Launcher.Plugin.ShortcutPlugin.Models.Shortcuts;
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
    public void Setup()
    {
        CreateEmptyFiles();
        InitializePlugin();
        SeedShortcuts();
    }

    [Test]
    public void ListCommand_ShouldReturnAllShortcuts()
    {
        var query = QueryExtensions.BuildQuery("q list");
        var results = _plugin.Query(query);

        var shortcutsCount = _shortcutsRepository.GetShortcuts().Count;

        Assert.That(results.Count - 1, Is.EqualTo(shortcutsCount));

        TestContext.Out.WriteLine($"Shortcuts count: {shortcutsCount}");
        TestContext.Out.WriteLine($"Results count: {results.Count}");
    }

    [Test]
    [TestCase("add_dir_test")]
    public void AddDirectoryCommand_ShouldAddShortcut(string key)
    {
        var before = _shortcutsRepository.GetShortcuts(key) ?? new List<Shortcut>();

        InvokeCommand($"q add directory {key} \"C:\\\"");

        var after = _shortcutsRepository.GetShortcuts(key) ?? new List<Shortcut>();

        Assert.That(after, Has.Count.EqualTo(before.Count + 1));
    }

    [Test]
    [TestCase("add_url_test")]
    public void AddUrlCommand_ShouldAddShortcut(string key)
    {
        var before = _shortcutsRepository.GetShortcuts(key) ?? new List<Shortcut>();

        InvokeCommand($"q add url {key} \"https://www.google.com\"");

        var after = _shortcutsRepository.GetShortcuts(key) ?? new List<Shortcut>();

        Assert.That(after, Has.Count.EqualTo(before.Count + 1));
    }

    [Test]
    [TestCase("add_file_test")]
    public void AddFileCommand_ShouldAddShortcut(string key)
    {
        var before = _shortcutsRepository.GetShortcuts(key) ?? new List<Shortcut>();

        InvokeCommand($"q add file {key} \"C:\\test.txt\"");

        var after = _shortcutsRepository.GetShortcuts(key) ?? new List<Shortcut>();

        Assert.That(after, Has.Count.EqualTo(before.Count + 1));
    }

    [Test]
    [TestCase("add_shell_cmd_test", "cmd")]
    [TestCase("add_shell_powershell_test", "powershell")]
    public void AddShellCommand_ShouldAddShortcut(string key, string type)
    {
        var before = _shortcutsRepository.GetShortcuts(key) ?? new List<Shortcut>();

        InvokeCommand($"q add shell {type} {key} true \"cmd /c echo test\"");

        var after = _shortcutsRepository.GetShortcuts(key) ?? new List<Shortcut>();

        Assert.That(after, Has.Count.EqualTo(before.Count + 1));
    }


    [Test]
    public void RemoveCommand_ShouldRemoveShortcut()
    {
        var removeShortcut = _shortcutsRepository.GetShortcuts().First();

        InvokeCommand("q remove " + removeShortcut.Key);

        var after = _shortcutsRepository.GetShortcuts().Contains(removeShortcut);

        Assert.That(after, Is.False);
    }

    [Test]
    public void DuplicateCommand_ShouldDuplicateShortcut()
    {
        var duplicateShortcut = _shortcutsRepository.GetShortcuts().First();
        const string duplicateShortcutKey = "test";

        InvokeCommand($"q duplicate {duplicateShortcut.Key} {duplicateShortcutKey}");

        var after = _shortcutsRepository.GetShortcuts()
                                        .FirstOrDefault(x =>
                                            x.Key == duplicateShortcutKey &&
                                            x.GetType() == duplicateShortcut.GetType());

        Assert.That(after, Is.Not.Null);
    }

    private void InvokeCommand(string rawQuery)
    {
        var query = QueryExtensions.BuildQuery(rawQuery);

        _plugin.Query(query)
               .ForEach(result => result.Action.Invoke(null));
    }

    private void InitializePlugin()
    {
        var pluginInitContext = new PluginInitContext(
            new PluginMetadata
            {
                ActionKeyword = "q",
                ActionKeywords = new List<string> {"q"}
            },
            new PublicApi(_shortcutsPath, _variablesPath)
        );

        _plugin = new ShortcutPlugin();
        _plugin.Init(pluginInitContext);

        _shortcutsRepository = _plugin.ServiceProvider.GetService<IShortcutsRepository>()!;
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