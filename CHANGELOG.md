# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

- Add quotes escaping

## [1.1.4] - 2024-02-04

- Shorcut name highlighting in the search results
- Ability to set a specific application to open file or URL shortcuts. Add "App" field in the shortcut JSON file. Provide application name (works only for registered applications in the system) or full path to the executable file. If the application is not defined, the default application will be used to open the file or URL. Example:

  ```json
  {
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
  },
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
