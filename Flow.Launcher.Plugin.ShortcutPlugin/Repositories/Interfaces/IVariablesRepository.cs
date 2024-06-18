using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface IVariablesRepository
{
    public void AddVariable(string name, string value);
    public void RemoveVariable(string name);
    public void UpdateVariable(string name, string value);
    public Variable GetVariable(string name);
    public List<Variable> GetVariables();
    public void Reload();
    public string ExpandVariables(string value);
    public void ImportVariables(string path);
    public void ExportVariables(string path);
}