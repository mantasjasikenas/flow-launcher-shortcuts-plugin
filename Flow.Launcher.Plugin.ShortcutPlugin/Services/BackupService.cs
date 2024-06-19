using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Services;

public class BackupService : IBackupService
{
    private readonly IBackupRepository _backupRepository;

    public BackupService(IBackupRepository backupRepository)
    {
        _backupRepository = backupRepository;
    }

    public List<Result> Backup()
    {
        return ResultExtensions.SingleResult(
            "Create a backup",
            "Your current shortcuts and variables will be backed up",
            () => { _backupRepository.Backup(); });
    }

    public List<Result> Restore()
    {
        var backups = _backupRepository.GetBackups();

        if (backups.Count == 0)
        {
            return ResultExtensions.SingleResult("No backups found", "No backups found");
        }

        var results = backups
                      .Select(backup =>
                      {
                          return ResultExtensions.Result(
                              backup.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                              backup.Path,
                              () => { _backupRepository.Restore(backup.Path); });
                      })
                      .OrderByDescending(x => x.Title)
                      .ToList();

        results.Insert(0,
            ResultExtensions.Result("Select a backup to restore", "Shortcuts and variables will be restored"));

        return results;
    }

    public List<Result> GetBackups()
    {
        var backups = _backupRepository.GetBackups();

        if (backups.Count == 0)
        {
            return ResultExtensions.SingleResult("No backups found", "No backups found");
        }

        return backups
               .Select(backup =>
               {
                   return ResultExtensions.Result(
                       backup.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                       backup.Path,
                       () => { ShortcutUtilities.OpenDirectory(backup.Path); }
                   );
               })
               .OrderByDescending(x => x.Title)
               .ToList();
    }

    public List<Result> DeleteBackup()
    {
        var backups = _backupRepository.GetBackups();

        if (backups.Count == 0)
        {
            return ResultExtensions.SingleResult("No backups found", "No backups found");
        }

        var results = backups
                      .Select(backup =>
                      {
                          return ResultExtensions.Result(
                              backup.DateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                              backup.Path,
                              () => { _backupRepository.DeleteBackup(backup.Path); });
                      })
                      .OrderByDescending(x => x.Title)
                      .ToList();

        results.Insert(0,
            ResultExtensions.Result("Select a backup to delete", "The selected backup will be deleted"));

        return results;
    }

    public List<Result> ClearBackups()
    {
        return ResultExtensions.SingleResult(
            "Clear all backups",
            "All backups will be deleted",
            () => { _backupRepository.ClearBackups(); });
    }
}