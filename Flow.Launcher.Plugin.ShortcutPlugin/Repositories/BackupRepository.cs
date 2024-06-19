using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Utilities;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class BackupRepository : IBackupRepository
{
    private readonly PluginInitContext _context;

    private readonly ISettingsService _settingsService;

    private readonly IShortcutsRepository _shortcutsRepository;

    private readonly IVariablesRepository _variablesRepository;

    private static string BackupPath => Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) +
                                        "\\ShortcutPlugin";

    public BackupRepository(PluginInitContext context, ISettingsService settingsService,
        IShortcutsRepository shortcutsRepository, IVariablesRepository variablesRepository)
    {
        _context = context;
        _settingsService = settingsService;
        _shortcutsRepository = shortcutsRepository;
        _variablesRepository = variablesRepository;

        Initialize();
    }

    private static void Initialize()
    {
        if (!Directory.Exists(BackupPath))
        {
            Directory.CreateDirectory(BackupPath);
        }
    }

    public void Backup()
    {
        var timestamp = DateTime.Now.ToString(Constants.BackupTimestampFormat);
        var backupFolderPath = BackupPath + "\\" + "backup_" + timestamp;

        Directory.CreateDirectory(backupFolderPath);

        var shortcutsPath = _settingsService.GetSettingOrDefault(x => x.ShortcutsPath);
        var variablesPath = _settingsService.GetSettingOrDefault(x => x.VariablesPath);

        File.Copy(shortcutsPath, backupFolderPath + "\\shortcuts.json");
        File.Copy(variablesPath, backupFolderPath + "\\variables.json");
    }

    public void Restore(string path)
    {
        _shortcutsRepository.ImportShortcuts(path + "\\shortcuts.json");
        _variablesRepository.ImportVariables(path + "\\variables.json");
    }

    public void DeleteBackup(string path)
    {
        Directory.Delete(path, true);
    }

    public void ClearBackups()
    {
        Directory.Delete(BackupPath, true);
        Initialize();
    }

    public IList<Backup> GetBackups()
    {
        return Directory
               .GetDirectories(BackupPath)
               .Select(x =>
               {
                   var timestamp = x.Split("_").Last();
                   var date = DateTime.ParseExact(timestamp, Constants.BackupTimestampFormat, null);

                   return new Backup
                   {
                       Path = x,
                       DateTime = date
                   };
               })
               .OrderByDescending(x => x.DateTime)
               .ToList();
    }
}