using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

public class BackupCommand : ICommand
{
    private readonly IBackupService _backupService;

    public BackupCommand(IBackupService backupService)
    {
        _backupService = backupService;
    }

    public Command Create()
    {
        return CreateBackupCommand();
    }

    private Command CreateBackupCommand()
    {
        var listBackup = new ArgumentLiteralBuilder()
                         .WithKey("list")
                         .WithResponseInfo(("backup list", "Show all backups"))
                         .WithResponseFailure(("Failed to list backup", "Something went wrong"))
                         .WithResponseSuccess(("List", "List all backups"))
                         .WithHandler(ListBackupCommandHandler)
                         .Build();

        var createBackup = new ArgumentLiteralBuilder()
                           .WithKey("create")
                           .WithResponseInfo(("backup create", "Create new backup"))
                           .WithResponseFailure(("Failed to create backup", "Something went wrong"))
                           .WithResponseSuccess(("backup created", "backup created successfully"))
                           .WithHandler(BackupCommandHandler)
                           .Build();

        var restoreBackup = new ArgumentLiteralBuilder()
                            .WithKey("restore")
                            .WithResponseInfo(("backup restore", "Restore selected backup"))
                            .WithResponseFailure(("Failed to restore backup", "Something went wrong"))
                            .WithResponseSuccess(("backup restored", "backup restored successfully"))
                            .WithHandler(RestoreCommandHandler)
                            .Build();


        var deleteBackup = new ArgumentLiteralBuilder()
                           .WithKey("delete")
                           .WithResponseInfo(("backup delete", "Delete selected backup"))
                           .WithResponseFailure(("Failed to delete backup", "Something went wrong"))
                           .WithResponseSuccess(("backup deleted", "backup deleted successfully"))
                           .WithHandler(DeleteBackupCommandHandler)
                           .Build();

        var clearBackup = new ArgumentLiteralBuilder()
                          .WithKey("clear")
                          .WithResponseInfo(("backup clear", "Clear all backups"))
                          .WithResponseFailure(("Failed to clear backup", "Something went wrong"))
                          .WithResponseSuccess(("backup cleared", "backup cleared successfully"))
                          .WithHandler(ClearBackupCommandHandler)
                          .Build();

        return new CommandBuilder()
               .WithKey("backup")
               .WithResponseInfo(("backup", "Manage backup"))
               .WithResponseFailure(("Failed to manage backup", "Something went wrong"))
               .WithArguments(listBackup, createBackup, restoreBackup, deleteBackup, clearBackup)
               .Build();
    }

    private List<Result> ClearBackupCommandHandler(ActionContext context, List<string> arguments)
    {
        return _backupService.ClearBackups();
    }

    private List<Result> DeleteBackupCommandHandler(ActionContext context, List<string> arguments)
    {
        return _backupService.DeleteBackup();
    }

    private List<Result> ListBackupCommandHandler(ActionContext context, List<string> arguments)
    {
        return _backupService.GetBackups();
    }

    private List<Result> BackupCommandHandler(ActionContext context, List<string> arguments)
    {
        return _backupService.Backup();
    }

    private List<Result> RestoreCommandHandler(ActionContext context, List<string> arguments)
    {
        return _backupService.Restore();
    }
}