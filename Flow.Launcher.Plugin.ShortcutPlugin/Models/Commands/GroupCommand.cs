using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.ShortcutPlugin.Extensions;
using Flow.Launcher.Plugin.ShortcutPlugin.Helper;
using Flow.Launcher.Plugin.ShortcutPlugin.Repositories.Interfaces;
using Flow.Launcher.Plugin.ShortcutPlugin.Services.Interfaces;

namespace Flow.Launcher.Plugin.ShortcutPlugin.Models.Commands;

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
        var addGroupGroupLaunchArgument = CreateAddGroupGroupLaunchArgument(addGroupKeysArgument);
        var addGroupNameArgument = CreateAddGroupNameArgument(addGroupGroupLaunchArgument);

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

    private Argument CreateAddGroupNameArgument(IQueryExecutor addGroupKeysArgument)
    {
        return new ArgumentBuilder()
               .WithResponseInfo(("Enter group name", "How should your group be named?"))
               .WithArguments(addGroupKeysArgument)
               .Build();
    }

    private Argument CreateAddGroupGroupLaunchArgument(IQueryExecutor addGroupKeysArgument)
    {
        return new ArgumentBuilder()
               .WithResponseInfo(("Enable group launch", "Should the group be launched as a group? (true/false)"))
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

    private List<Result> ListGroupsCommandHandler(ActionContext context,ParsedQuery parsedQuery)
    {
        return _shortcutsService.GetGroupsList();
    }

    private List<Result> RemoveGroupCommandHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var arguments = parsedQuery.CommandArguments;
        return _shortcutsService.RemoveGroup(arguments[2]);
    }

    private List<Result> AddGroupCommandHandler(ActionContext context, ParsedQuery parsedQuery)
    {
        var arguments = parsedQuery.CommandArguments;
        var launchGroup = !bool.TryParse(arguments[3], out var groupLaunch) || groupLaunch;
        var keys = arguments.Skip(4).ToList();
        var key = arguments[2];

        return ResultExtensions.SingleResult("Creating group shortcut",
            $"Group launch : {launchGroup.ToString().ToLowerInvariant()}, keys : {string.Join(", ", keys)}",
            () => { _shortcutsRepository.GroupShortcuts(key, launchGroup, keys); });
    }
}