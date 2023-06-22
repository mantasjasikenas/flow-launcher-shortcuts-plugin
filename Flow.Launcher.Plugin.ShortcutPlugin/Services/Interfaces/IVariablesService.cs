using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IVariablesService
{
    List<Result> GetVariables();
    List<Result> GetVariable(string name);
    List<Result> AddVariable(string name, string value);
    List<Result> RemoveVariable(string name);
    List<Result> UpdateVariable(string name, string value);
    void Reload();
    string ExpandVariables(string value);
}