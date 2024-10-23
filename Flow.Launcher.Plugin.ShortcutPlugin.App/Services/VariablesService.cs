using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;
using Constants = Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers.Constants;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Services;

public class VariablesService : IVariablesService
{
    private const string TestVariablesPath = """C:\Users\tutta\AppData\Roaming\FlowLauncher\Settings\Plugins\Flow.Launcher.Plugin.VariablePlugin\Backups\20240925185136957\variables.json""";

    private readonly ILocalSettingsService _localSettingsService;

    private Dictionary<string, Variable> _variables = [];


    public VariablesService(ILocalSettingsService localSettingsService)
    {
        Task.Run(RefreshVariablesAsync);
        _localSettingsService = localSettingsService;
    }

    private async Task<Dictionary<string, Variable>> ReadVariables()
    {
        var path = await GetVariablesPath();

        if (string.IsNullOrEmpty(path))
        {
            return [];
        }

        return await VariableUtilities.ReadVariables(path);
    }

    private async Task SaveVariables(Dictionary<string, Variable> variables)
    {
        var path = await GetVariablesPath();

        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        VariableUtilities.SaveVariables([.. variables.Values], path);
    }

    private async Task<string> GetVariablesPath()
    {
        return await _localSettingsService.ReadSettingAsync<string>(Constants.VariablesPathKey) ?? TestVariablesPath;
    }


    public async Task<IEnumerable<Variable>> GetVariablesAsync()
    {
        if (_variables.Count == 0)
        {
            _variables = await ReadVariables();
        }

        return [.. _variables.Values];
    }

    public async Task RefreshVariablesAsync()
    {
        _variables = await ReadVariables();
    }

    public async Task SaveVariableAsync(Variable variable)
    {
        var key = variable.Name;

        _variables[key] = variable;

        await SaveVariables(_variables);
    }

    public async Task DeleteVariableAsync(Variable variable)
    {
        var key = variable.Name;

        if (_variables.Remove(key))
        {
            await SaveVariables(_variables);
        }
    }

    public async Task UpdateVariableAsync(Variable oldVariable, Variable updatedVariable)
    {
        var oldKey = oldVariable.Name;
        var newKey = updatedVariable.Name;

        _variables.Remove(oldKey);
        _variables[newKey] = updatedVariable;

        await SaveVariables(_variables);
    }
}
