using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IVariablesService
{
    List<Result> GetVariablesList();
    List<Result> GetVariable(string name);
    List<Result> AddVariable(string name, string value);
    List<Result> RemoveVariable(string name);
    List<Result> UpdateVariable(string name, string value);
    Task ReloadAsync();
    string ExpandVariables(string value);
    string ExpandVariables(string value, IReadOnlyDictionary<string, string> arguments);
    List<Result> ImportVariables();
    List<Result> ExportVariables();
}