using System.Collections.Generic;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

public interface IBackupService
{
    List<Result> Backup();
    List<Result> Restore();
    List<Result> GetBackups();
    List<Result> DeleteBackup();
    List<Result> ClearBackups();
}