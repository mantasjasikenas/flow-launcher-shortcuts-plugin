using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Contracts.Services;

public interface IVariablesService
{
    Task<IEnumerable<Variable>> GetVariablesAsync();
    Task SaveVariableAsync(Variable variable);
    Task DeleteVariableAsync(Variable variable);
    Task UpdateVariableAsync(Variable oldVariable, Variable updatedVariable);
    Task RefreshVariablesAsync();
}
