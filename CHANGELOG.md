﻿# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

- Editor app

## [1.2.3] - 2025-xx-xx

- Removed default title autocompletion for commands. Invoking autocompletion will replace the query with the same query
  text.

## [1.2.2] - 2025-01-22

- Support for shell type `Pwsh`.
- If action is not specified for query result, pressing `Enter` will not close the launcher.
- Snippet shortcut type. Executing a snippet shortcut will copy the snippet value to the clipboard. Better would be if
  executing a snippet shortcut will paste the snippet value to the active window but didn't find a way to do that so
  far. Example:

  ```json
  {
    "Type": "Snippet",
    "Value": "Hello\rworld",
    "Key": "hi"
  }
  ```
- Autocompleting on Shell shortcut type changes the query to shell plugin keyword followed by the shell shortcut
  arguments.
- Changed the default behavior of list commands (`q list`, `q snippets`, `q group list`). Now invoking item from the
  list will replace the query with the selected item.
- Removed default argument assignment when shortcut had only one argument. Now the user has to provide the argument name
  when invoking the shortcut. This change was made due conflicts with other functionality.
- Added positional arguments support. For example, you have URL shortcut with the following arguments:

  ```json
  {
    "Type": "Url",
    "Url": "https://www.youtube.com/watch?v=${value}&list=${list}",
    "Key": "yt"
  }
  ```

  Before, you had to provide the arguments in the following way: `q yt -value watch -list later`. Now you can provide
  the arguments in the following way: `q yt -1 watch -2 later`. The numbers represent the argument position in the
  shortcut. Numbering starts from 1.
- Fixed group shortcuts invocation bug when the group shortcut name contained spaces.

## [1.2.1] - 2024-09-24

- Do not autocomplete the result when Enter is pressed if the argument is not a literal type.
- Added a `LaunchGroup` field to group shortcuts. If set to `true`, you can launch all shortcuts in the group
  simultaneously. Otherwise, you can only launch a single shortcut from the group at a time.
- When the group shortcut name (key) is not fully typed, selecting the group shortcut from the list will replace the
  query with the group shortcut name.
- Icons folder support: If a directory type shortcut with the key `icons` is added, the plugin will search for icons for
  shortcuts based on the shortcut key matching the icon file name. The icon file name should be the same as the
  shortcut. Supported file extensions: `.png`, `.jpg`, `.jpeg`, `.ico`.

## [1.2.0] - 2024-09-16

- Fix group creation bug
- Show available possible variables when using `q var remove` command
- Show available possible shortcuts when using `q remove` command

## [1.1.9] - 2024-09-16

- QuickLook support to preview folders and files
- Improved code quality and performance
- Enchanted config command. Now shows icon of the application which will be used to open configuration files

## [1.1.8] - 2024-09-12

- Add autocomplete support for commands with arguments
- After typing the full shortcut, for example `q dev`, pressing `Ctr+Tab` (autocomplete) will replace current query with
  the shortcut path
- Alias support for shortcuts. Add `Alias` field in the shortcut JSON file. Provide the alias name. Example:

  ```json
  {
    "Type": "Directory",
    "Path": "C:\\Users\\tutta\\Storage\\Dev",
    "Key": "dev",
    "Alias": ["development", "devs"]
  }
  ```
- Show associated icons for shortcuts in the search results
- New plugin icon
- Customizable icons for shortcuts. In order to use custom icons, add `Icon` field in the shortcut JSON file.
  Provide the full path to the icon file. Example:

  ```json
    {
    "Type": "Directory",
    "Path": "C:\\Users\\tutta\\Storage\\Dev",
    "Key": "dev",
    "Icon": "C:\\Users\\tutta\\Storage\\Dev\\Projects\\ShortcutPlugin\\Flow.Launcher.Plugin.ShortcutPlugin\\Images\\discord-mark-white.png"
  }
  ```

## [1.1.7] - 2024-06-21

- New backup command with subcommands.
- Added copy result title and subtitle options in the context menu
- Fix issue with cmd arguments quotes
- Support for VS Code and VS Code - Insiders in context menu for folders
- Help command with documentation, issues links and developer Discord username

## [1.1.6] - 2024-05-08

- Improved possible shortcuts fuzzy search. Now fuzzy search is case insensitive.
- Version command to see the current plugin version.
- Fix the disappearing config file after updating the plugin using default shortcuts and variables paths. This should
  start working updating from **v1.1.6** to future releases.
- Changed default shortcuts and variable paths.
- Reset button in the settings panel to set default settings values.

## [1.1.5] - 2024-04-05

- Invoking command from command list will change query text to the command name
- Report command to open GitHub issues
- Show count of shortcuts in list command
- Multiple shortcuts with the same key support
- Improved group shortcuts invocation

## [1.1.4] - 2024-02-04

- Shortcut name highlighting in the search results
- Ability to set a specific application to open file or URL shortcuts. Add "App" field in the shortcut JSON file.
  Provide application name (works only for registered applications in the system) or full path to the executable file.
  If the application is not defined, the default application will be used to open the file or URL. Example:

  ```json
  [{
    "Type": "Url",
    "Url": "https://www.youtube.com/playlist?list=WL",
    "App": "msedge.exe",
    "Key": "wl2"
  },
  {
    "Type": "File",
    "App": "C:\\Program Files\\WindowsApps\\Microsoft.WindowsNotepad_11.2312.18.0_x64__8wekyb3d8bbwe\\Notepad\\Notepad.exe",
    "Path": "C:\\Users\\tutta\\Storage\\Motion Picture\\labas.txt",
    "Key": "t1"
  },
  {
    "Type": "File",
    "App": "notepad",
    "Path": "C:\\Users\\tutta\\Storage\\Motion Picture\\labas.txt",
    "Key": "t2"
  }]
  ```

## [1.1.3] - 2024-01-31

- New shortcut type: `shell`. This type allows you to execute commands in command prompt or powershell.
- Improved auto completion for commands.

## [1.1.2] - 2024-01-22

- Fix group shortcut invocation

## [1.1.1] - 2024-01-18

- Multiple plugin keywords support. Now you can use:
    - `q keyword get` to list all plugin keywords for the plugin.
    - `q keyword set <keyword>` to set the plugin keyword. This will override any existing keywords for the plugin.
    - `q keyword add <keyword>` to add additional keyword for the plugin.
    - `q keyword remove <keyword>` to remove a keyword from the plugin.
- Provided argument without name will be assigned to the first argument of the shortcut.

## [1.1.0] - 2023-12-17

- Auto-completion support
- Fuzzy search for shortcuts
- Improved search result list

## [1.0.9] - 2023-11-30

- Allow dynamic arguments in command q list
- Improve handling of shortcut expansion with arguments

## [1.0.8] - 2023-11-02

- Shortcut completion suggestions while typing
- Context menu for directories and files

## [1.0.7] - 2023-09-28

- Fixed bug when creating a new shortcut (missing directory)
- Updated group shortcut positioning

## [1.0.6] - 2023-09-05

- New shortcut icons
- Modified group shortcut displaying

## [1.0.5] - 2023-08-25

- New commands handling
- Pass dynamic arguments to shortcut
- Typing suggestions, and information for the user
- Multiple shortcut groups

## [1.0.4] - 2023-08-20

- Fixed missing icon

## [1.0.3] - 2023-08-13

- Fixed shortcuts deserialization bug
- Updated README.md logo

## [1.0.2] - 2023-08-11

- Modified project structure.
- Bumped target framework to NET 7.0

## [1.0.1] - 2023-06-07

- Added support for files shortcuts.

## [1.0.0] - 2023-06-06

- Initial shortcuts manager release for Flow Launcher.
