using Flow.Launcher.Plugin.ShortcutPlugin.App.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.App.Helpers;
public static class VariableExtensions
{
    public static ObservableVariable ToObservableVariable(this Variable variable)
    {
        return new ObservableVariable(variable);
    }

    public static Variable ToVariable(this ObservableVariable observableVariable)
    {
        return observableVariable.GetBaseVariable();
    }
}
