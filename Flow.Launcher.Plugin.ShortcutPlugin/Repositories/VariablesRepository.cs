using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using FuzzySharp;
using CommonVariableUtilities = Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper.VariableUtilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class VariablesRepository : IVariablesRepository
{
    private readonly ISettingsService _settingsService;

    private readonly IPluginManager _pluginManager;

    private Dictionary<string, Variable> _variables;


    public VariablesRepository(ISettingsService settingsService, IPluginManager pluginManager)
    {
        _settingsService = settingsService;
        _pluginManager = pluginManager;
    }

    public async Task InitializeAsync()
    {
        _variables = await ReadVariables(VariablesPath);
    }

    private string VariablesPath => _settingsService.GetSettingOrDefault(x => x.VariablesPath);

    public List<Variable> GetVariables()
    {
        return _variables.Values.ToList();
    }

    public IEnumerable<Variable> GetPossibleVariables(string name)
    {
        name = name.ToLowerInvariant();

        return GetVariables()
               .Where(x => Fuzz.PartialRatio(x.Name, name) > 90)
               .OrderByDescending(x => Fuzz.Ratio(x.Name, name))
               .Distinct()
               .ToList();
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

    public async Task ReloadVariablesAsync()
    {
        _variables = await ReadVariables(VariablesPath);
    }

    public string ExpandVariables(string value)
    {
        var result = _variables.Aggregate(value,
            (current, variable) =>
                current.Replace(string.Format(Constants.VariableFormat, variable.Key), variable.Value.Value));

        return Environment.ExpandEnvironmentVariables(result);
    }

    public async Task ImportVariables(string path)
    {
        try
        {
            var variables = await ReadVariables(path);

            if (variables.Count == 0)
            {
                throw new Exception("No valid variables found in the file.");
            }

            _variables = variables;

            SaveVariables();
            await ReloadVariablesAsync();

            _pluginManager.API.ShowMsg("Variables imported successfully");
        }
        catch (Exception ex)
        {
            _pluginManager.API.ShowMsg("Error while importing variables");
            _pluginManager.API.LogException(nameof(VariablesRepository), "Error importing variables", ex);
        }
    }

    public Task ExportVariables(string path)
    {
        if (!File.Exists(_settingsService.GetSettingOrDefault(x => x.VariablesPath)))
        {
            _pluginManager.API.ShowMsg("No variables to export");
            return Task.CompletedTask;
        }

        try
        {
            File.Copy(_settingsService.GetSettingOrDefault(x => x.VariablesPath), path);
        }
        catch (Exception ex)
        {
            _pluginManager.API.ShowMsg("Error while exporting variables");
            _pluginManager.API.LogException(nameof(VariablesRepository), "Error exporting variables", ex);
        }

        return Task.CompletedTask;
    }

    private static async Task<Dictionary<string, Variable>> ReadVariables(string path)
    {
        return await CommonVariableUtilities.ReadVariables(path);
    }

    private void SaveVariables()
    {
        CommonVariableUtilities.SaveVariables(_variables.Values.ToList(), VariablesPath);
    }
}