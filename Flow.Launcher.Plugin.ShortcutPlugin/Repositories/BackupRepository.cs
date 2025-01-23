using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Common.Models;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Repositories;

public class BackupRepository : IBackupRepository
{
    private readonly IPluginManager _pluginManager;

    private readonly ISettingsService _settingsService;

    private readonly IShortcutsRepository _shortcutsRepository;

    private readonly IVariablesRepository _variablesRepository;

    private string BackupPath => GetBackupPath();

    public BackupRepository(IPluginManager pluginManager, ISettingsService settingsService,
        IShortcutsRepository shortcutsRepository, IVariablesRepository variablesRepository)
    {
        _pluginManager = pluginManager;
        _settingsService = settingsService;
        _shortcutsRepository = shortcutsRepository;
        _variablesRepository = variablesRepository;

        Initialize();
    }

    private void Initialize()
    {
        if (!Directory.Exists(BackupPath))
        {
            Directory.CreateDirectory(BackupPath);
        }
    }

    private string GetBackupPath()
    {
        var pluginDirectory = _pluginManager.Metadata.PluginDirectory;
        var parentDirectory = Directory.GetParent(pluginDirectory)?.Parent?.FullName;

        if (parentDirectory == null)
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\ShortcutPlugin\\" +
                   Constants.BackupsFolder;
        }

        return Path.Combine(parentDirectory, Constants.PluginDataPath, Constants.BackupsFolder);
    }

    public void Backup()
    {
        var timestamp = DateTime.Now.ToString(Constants.BackupTimestampFormat);
        var backupFolderPath = $"{BackupPath}\\{timestamp}";

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
                   var timestamp = x.Split('\\').Last();
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