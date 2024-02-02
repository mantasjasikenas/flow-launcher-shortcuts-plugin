using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.models;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin;

public class GroupCommand : ICommand
{
    private readonly IShortcutsService _shortcutsService;
    private readonly IShortcutsRepository _shortcutsRepository;

    public GroupCommand(IShortcutsService shortcutsService, IShortcutsRepository shortcutsRepository)
    {
        _shortcutsService = shortcutsService;
        _shortcutsRepository = shortcutsRepository;
    }

    public Command Create()
    {
        return CreateGroupCommand();
    }

    private Command CreateGroupCommand()
    {
        var addSubCommand = CreateAddSubCommand();
        var removeSubCommand = CreateRemoveSubCommand();
        var listSubCommand = CreateListSubCommand();

        return new CommandBuilder()
            .WithKey("group")
            .WithResponseInfo(("group", "Manage shortcuts group"))
            .WithResponseFailure(("Failed to manage shortcuts group", "Something went wrong"))
            .WithArguments(addSubCommand, removeSubCommand, listSubCommand)
            .Build();
    }

    private ArgumentLiteral CreateAddSubCommand()
    {
        var addGroupKeysArgument = CreateAddGroupKeysArgument();
        var addGroupNameArgument = CreateAddGroupNameArgument(addGroupKeysArgument);

        return new ArgumentLiteralBuilder()
            .WithKey("add")
            .WithResponseInfo(("group add", "Add shortcuts group"))
            .WithResponseFailure(("Failed to add shortcuts group", "Something went wrong"))
            .WithArgument(addGroupNameArgument)
            .Build();
    }

    private Argument CreateAddGroupKeysArgument()
    {
        return new ArgumentBuilder()
            .WithResponseInfo(("Enter shortcuts keys", "Which shortcuts should be in the group?"))
            .WithHandler(AddGroupCommandHandler)
            .WithMultipleValuesForSingleArgument()
            .Build();
    }

    private Argument CreateAddGroupNameArgument(Argument addGroupKeysArgument)
    {
        return new ArgumentBuilder()
            .WithResponseInfo(("Enter group name", "How should your group be named?"))
            .WithResponseSuccess(("Add", "Your group will be added"))
            .WithArguments(addGroupKeysArgument)
            .Build();
    }

    private ArgumentLiteral CreateRemoveSubCommand()
    {
        var removeGroupNameArgument = new ArgumentBuilder()
            .WithResponseInfo(("Enter group name", "Which group should be removed?"))
            .WithResponseSuccess(("Remove", "Your group will be removed"))
            .WithHandler(RemoveGroupCommandHandler)
            .Build();

        return new ArgumentLiteralBuilder()
            .WithKey("remove")
            .WithResponseInfo(("group remove", "Remove shortcuts group"))
            .WithResponseFailure(("Failed to remove shortcuts group", "Something went wrong"))
            .WithArgument(removeGroupNameArgument)
            .Build();
    }

    private ArgumentLiteral CreateListSubCommand()
    {
        return new ArgumentLiteralBuilder()
            .WithKey("list")
            .WithResponseInfo(("group list", "List all shortcuts groups"))
            .WithResponseFailure(("Failed to list shortcuts groups", "Something went wrong"))
            .WithResponseSuccess(("List", "List all shortcuts groups"))
            .WithHandler(ListGroupsCommandHandler)
            .Build();
    }

    /*     private Command CreateGroupCommand()
        {
            var addGroupKeysArgument = new ArgumentBuilder()
                .WithResponseInfo(("Enter shortcuts keys", "Which shortcuts should be in the group?"))
                .WithHandler(AddGroupCommandHandler)
                .WithMultipleValuesForSingleArgument()
                .Build();

            var addGroupNameArgument = new ArgumentBuilder()
                .WithResponseInfo(("Enter group name", "How should your group be named?"))
                .WithResponseSuccess(("Add", "Your group will be added"))
                .WithArguments(addGroupKeysArgument)
                .Build();

            var addSubCommand = new ArgumentLiteralBuilder()
                .WithKey("add")
                .WithResponseInfo(("group add", "Add shortcuts group"))
                .WithResponseFailure(("Failed to add shortcuts group", "Something went wrong"))
                .WithArgument(addGroupNameArgument)
                .Build();

            var removeGroupNameArgument = new ArgumentBuilder()
                .WithResponseInfo(("Enter group name", "Which group should be removed?"))
                .WithResponseSuccess(("Remove", "Your group will be removed"))
                .WithHandler(RemoveGroupCommandHandler)
                .Build();

            var removeSubCommand = new ArgumentLiteralBuilder()
                .WithKey("remove")
                .WithResponseInfo(("group remove", "Remove shortcuts group"))
                .WithResponseFailure(("Failed to remove shortcuts group", "Something went wrong"))
                .WithArgument(removeGroupNameArgument)
                .Build();

            var listSubCommand = new ArgumentLiteralBuilder()
                .WithKey("list")
                .WithResponseInfo(("group list", "List all shortcuts groups"))
                .WithResponseFailure(("Failed to list shortcuts groups", "Something went wrong"))
                .WithResponseSuccess(("List", "List all shortcuts groups"))
                .WithHandler(ListGroupsCommandHandler)
                .Build();

            return new CommandBuilder()
                .WithKey("group")
                .WithResponseInfo(("group", "Manage shortcuts group"))
                .WithResponseFailure(("Failed to manage shortcuts group", "Something went wrong"))
                .WithArguments(addSubCommand, removeSubCommand, listSubCommand)
                .Build();
        } */

    private List<Result> ListGroupsCommandHandler(ActionContext context, List<string> arguments)
    {
        return _shortcutsService.GetGroups();
    }

    private List<Result> RemoveGroupCommandHandler(ActionContext context, List<string> arguments)
    {
        return _shortcutsService.RemoveShortcut(arguments[2]);
    }

    private List<Result> AddGroupCommandHandler(ActionContext context, List<string> arguments)
    {
        var keys = arguments.Skip(3).ToList();

        return ResultExtensions.SingleResult("Creating group shortcut", "Keys : " + string.Join(", ", keys), () =>
        {
            var key = arguments[2];
            _shortcutsRepository.GroupShortcuts(key, keys);
        });
    }
}
