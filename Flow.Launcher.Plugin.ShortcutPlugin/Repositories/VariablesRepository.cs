using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class VariablesRepository : IVariablesRepository
{
    private readonly PluginInitContext _context;
    private readonly ISettingsService _settingsService;

    private Dictionary<string, Variable> _variables;
    private string VariablesPath => _settingsService.GetSetting(x => x.VariablesPath);


    public VariablesRepository(PluginInitContext context, ISettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
        _variables = ReadVariablesFile(VariablesPath);
    }

    public List<Variable> GetVariables()
    {
        return _variables.Values.ToList();
    }

    public Variable GetVariable(string name)
    {
        return _variables.TryGetValue(name, out var variable) ? variable : null;
    }

    public void AddVariable(string name, string value)
    {
        _variables[name] = new Variable {Name = name, Value = value};
        SaveVariablesToFile();
    }

    public void RemoveVariable(string name)
    {
        _variables.Remove(name);
        SaveVariablesToFile();
    }

    public void UpdateVariable(string name, string value)
    {
        _variables[name] = new Variable {Name = name, Value = value};
        SaveVariablesToFile();
    }

    public void Reload()
    {
        _variables = ReadVariablesFile(VariablesPath);
    }

    private Dictionary<string, Variable> ReadVariablesFile(string path)
    {
        if (!File.Exists(path)) return new Dictionary<string, Variable>();

        try
        {
            var variables = JsonSerializer.Deserialize<List<Variable>>(File.ReadAllText(path));
            return variables.ToDictionary(shortcut => shortcut.Name);
        }
        catch (Exception)
        {
            return new Dictionary<string, Variable>();
        }
    }

    private void SaveVariablesToFile()
    {
        var options = new JsonSerializerOptions {WriteIndented = true};
        var json = JsonSerializer.Serialize(_variables.Values, options);

        File.WriteAllText(VariablesPath, json);
    }
}