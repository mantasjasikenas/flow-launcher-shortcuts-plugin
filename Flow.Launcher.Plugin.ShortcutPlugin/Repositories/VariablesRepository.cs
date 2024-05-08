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
    private readonly ISettingsService _settingsService;

    private readonly PluginInitContext _context;

    private Dictionary<string, Variable> _variables;


    public VariablesRepository(ISettingsService settingsService, PluginInitContext context)
    {
        _settingsService = settingsService;
        _context = context;
        _variables = ReadVariables(VariablesPath);
    }

    private string VariablesPath => _settingsService.GetSettingOrDefault(x => x.VariablesPath);

    public List<Variable> GetVariables()
    {
        return _variables.Values.ToList();
    }

    public Variable GetVariable(string name)
    {
        return _variables.GetValueOrDefault(name);
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

    public void ImportVariables(string path)
    {
        try
        {
            var variables = ReadVariables(path);

            if (variables.Count == 0)
            {
                throw new Exception("No valid variables found in the file.");
            }

            _variables = variables;

            SaveVariables();
            Reload();

            _context.API.ShowMsg("Variables imported successfully");
        }
        catch (Exception ex)
        {
            _context.API.ShowMsg("Error while importing variables");
            _context.API.LogException(nameof(VariablesRepository), "Error importing variables", ex);
        }
    }

    public void ExportVariables(string path)
    {
        if (!File.Exists(_settingsService.GetSettingOrDefault(x => x.VariablesPath)))
        {
            _context.API.ShowMsg("No variables to export");
            return;
        }

        try
        {
            File.Copy(_settingsService.GetSettingOrDefault(x => x.VariablesPath), path);
        }
        catch (Exception ex)
        {
            _context.API.ShowMsg("Error while exporting variables");
            _context.API.LogException(nameof(VariablesRepository), "Error exporting variables", ex);
        }
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
            return variables.ToDictionary(variable => variable.Name);
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