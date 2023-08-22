using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class VariablesRepository : IVariablesRepository
{
    private readonly PluginInitContext _context;
    private readonly ISettingsService _settingsService;

    private Dictionary<string, Variable> _variables;


    public VariablesRepository(PluginInitContext context, ISettingsService settingsService)
    {
        _context = context;
        _settingsService = settingsService;
        _variables = ReadVariables(VariablesPath);
    }

    private string VariablesPath => _settingsService.GetSetting(x => x.VariablesPath);

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
        SaveVariables();
    }

    public void RemoveVariable(string name)
    {
        _variables.Remove(name);
        SaveVariables();
    }

    public void UpdateVariable(string name, string value)
    {
        _variables[name] = new Variable {Name = name, Value = value};
        SaveVariables();
    }

    public void Reload()
    {
        _variables = ReadVariables(VariablesPath);
    }

    public string ExpandVariables(string value)
    {
        var result = _variables.Aggregate(value,
            (current, variable) =>
                current.Replace(string.Format(Constants.VariableFormat, variable.Key), variable.Value.Value));

        return Environment.ExpandEnvironmentVariables(result);
    }

    private static Dictionary<string, Variable> ReadVariables(string path)
    {
        if (!File.Exists(path))
        {
            return new Dictionary<string, Variable>();
        }

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

    private void SaveVariables()
    {
        var options = new JsonSerializerOptions {WriteIndented = true};
        var json = JsonSerializer.Serialize(_variables.Values, options);

        File.WriteAllText(VariablesPath, json);
    }
}