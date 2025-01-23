using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;

public interface IBackupRepository
{
    void Backup();
    void Restore(string path);
    void DeleteBackup(string path);
    void ClearBackups();
    IList<Backup> GetBackups();
}