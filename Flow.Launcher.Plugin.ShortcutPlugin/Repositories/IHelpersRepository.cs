using System.Collections;
using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public interface IHelpersRepository
{
    public List<Helper> GetHelpers();
    public void Reload();
}