using Flow.Launcher.Plugin.ShortcutPlugin.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IQueryParser
{
    ParsedQuery Parse(Query query);
}