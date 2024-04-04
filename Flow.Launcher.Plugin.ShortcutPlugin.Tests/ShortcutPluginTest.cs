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
    public void AddCommand_ShouldAddShortcut()
    {
        var beforeCount = _shortcutsRepository.GetShortcuts().Count;

        InvokeCommand("q add directory root \"C:\\\"");

        var afterCount = _shortcutsRepository.GetShortcuts().Count;

        Assert.That(afterCount, Is.EqualTo(beforeCount + 1));
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