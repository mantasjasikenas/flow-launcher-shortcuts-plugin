using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IQueryInterpreter
{
    public List<Result> Interpret(Query query);
}