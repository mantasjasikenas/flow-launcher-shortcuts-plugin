using Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;
using Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Services;

public class VariablesService : IVariablesService
{
    private Dictionary<string, Variable> _variables = [];

    private readonly IPCManagerClient _iPCManagerClient;

    public VariablesService(IPCManagerClient iPCManagerClient)
    {
        Task.Run(RefreshVariablesAsync);
        _iPCManagerClient = iPCManagerClient;
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
        _ = _iPCManagerClient.SendMessageAsync(IPCCommand.ReloadPluginData.ToString(), CancellationToken.None);
    }

    private async Task<string> GetVariablesPath()
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var shortcutsPluginPath = Directory.GetParent(appDirectory)?.Parent?.FullName;

        return VariableUtilities.GetVariablesPath(shortcutsPluginPath);
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
