using System.Collections.Generic;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
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
        // backup addVariableValueArgument = new ArgumentBuilder()
        //                                .WithResponseInfo(
        //                                    ("Enter variable value", "What should your variable value be?"))
        //                                .WithHandler(AddVariableCommandHandler)
        //                                .Build();
        //
        // backup addVariableNameArgument = new ArgumentBuilder()
        //                               .WithResponseInfo(("Enter variable name", "How should your variable be named?"))
        //                               .WithArgument(addVariableValueArgument)
        //                               .Build();
        //
        // backup addVariable = new ArgumentLiteralBuilder()
        //                   .WithKey("add")
        //                   .WithResponseInfo(("backup add", "Add variable"))
        //                   .WithResponseFailure(("Failed to add variable", "Something went wrong"))
        //                   .WithArgument(addVariableNameArgument)
        //                   .Build();
        //
        // backup removeVariableArgument = new ArgumentBuilder()
        //                              .WithResponseInfo(("Enter variable name", "Which variable should be removed?"))
        //                              .WithResponseSuccess(("Remove", "Your variable will be removed from the list"))
        //                              .WithHandler(RemoveVariableCommandHandler)
        //                              .Build();
        //
        // backup removeVariable = new ArgumentLiteralBuilder()
        //                      .WithKey("remove")
        //                      .WithResponseInfo(("backup remove", "Remove variable"))
        //                      .WithResponseFailure(("Failed to remove variable", "Something went wrong"))
        //                      .WithArgument(removeVariableArgument)
        //                      .Build();

        var createBackup = new ArgumentLiteralBuilder()
                           .WithKey("create")
                           .WithResponseInfo(("backup create", "Create Backup"))
                           .WithResponseFailure(("Failed to create Backup", "Something went wrong"))
                           .WithResponseSuccess(("Backup created", "Backup created successfully"))
                           .WithHandler(BackupCommandHandler)
                           .Build();

        var restoreBackup = new ArgumentLiteralBuilder()
                            .WithKey("restore")
                            .WithResponseInfo(("backup restore", "Restore Backup"))
                            .WithResponseFailure(("Failed to restore Backup", "Something went wrong"))
                            .WithResponseSuccess(("Backup restored", "Backup restored successfully"))
                            .WithHandler(RestoreCommandHandler)
                            .Build();

        var listBackup = new ArgumentLiteralBuilder()
                         .WithKey("list")
                         .WithResponseInfo(("backup list", "List all Backup"))
                         .WithResponseFailure(("Failed to list Backup", "Something went wrong"))
                         .WithResponseSuccess(("List", "List all Backup"))
                         .WithHandler(ListBackupCommandHandler)
                         .Build();

        var deleteBackup = new ArgumentLiteralBuilder()
                           .WithKey("delete")
                           .WithResponseInfo(("backup delete", "Delete Backup"))
                           .WithResponseFailure(("Failed to delete Backup", "Something went wrong"))
                           .WithResponseSuccess(("Backup deleted", "Backup deleted successfully"))
                           .WithHandler(DeleteBackupCommandHandler)
                           .Build();

        var clearBackup = new ArgumentLiteralBuilder()
                          .WithKey("clear")
                          .WithResponseInfo(("backup clear", "Clear Backup"))
                          .WithResponseFailure(("Failed to clear Backup", "Something went wrong"))
                          .WithResponseSuccess(("Backup cleared", "Backup cleared successfully"))
                          .WithHandler(ClearBackupCommandHandler)
                          .Build();

        return new CommandBuilder()
               .WithKey("backup")
               .WithResponseInfo(("backup", "Manage Backup"))
               .WithResponseFailure(("Failed to manage Backup", "Something went wrong"))
               .WithArguments(listBackup, createBackup, restoreBackup, deleteBackup, clearBackup)
               .Build();
    }

    private List<Result> ClearBackupCommandHandler(ActionContext arg1, List<string> arg2)
    {
        return _backupService.ClearBackups();
    }

    private List<Result> DeleteBackupCommandHandler(ActionContext arg1, List<string> arg2)
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