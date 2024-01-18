﻿# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

## [1.1.1] - 2024-01-18

- Multiple plugin keywords support. Now you can use `q keyword add <keyword>` to add additional keywords for the plugin.
  For example, if you want to use `q` and `quick` as keywords for the plugin, you can use `q keyword add quick` to
  add `quick` as a keyword for the plugin. You can also use `q keyword remove <keyword>` to remove a keyword from the
  plugin.
- Argument name is now optional if only one argument is defined for the shortcut.

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