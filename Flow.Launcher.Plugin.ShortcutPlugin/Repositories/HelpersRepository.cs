using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Utils;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class HelpersRepository : IHelpersRepository
{
    private readonly PluginInitContext _context;
    private List<Helper> _helpers;

    public HelpersRepository(PluginInitContext context)
    {
        _context = context;
        _helpers = LoadHelpersFile();
    }

    public List<Helper> GetHelpers()
    {
        return _helpers;
    }

    public void Reload()
    {
        _helpers = LoadHelpersFile();
    }

    private List<Helper> LoadHelpersFile()
    {
        var fullPath = Path.Combine(_context.CurrentPluginMetadata.PluginDirectory, Constants.HelpersFileName);
        if (!File.Exists(fullPath)) return new List<Helper>();

        try
        {
            return JsonSerializer.Deserialize<List<Helper>>(File.ReadAllText(fullPath));
        }
        catch (Exception)
        {
            return new List<Helper>();
        }
    }
}