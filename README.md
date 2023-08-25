<div align="center">
  <img src="https://github.com/mantasjasikenas/flow-launcher-shortcuts-plugin/blob/master/assets/icon.png?raw=true" alt="Shortcuts logo" width="75">  
  <h1>Shortcuts Plugin <br> Quickly launch your shortcuts</h1>
  <br>
</div>

<div>
   <div>
      <a href="https://github.com/mantasjasikenas/flow-launcher-shortcuts-plugin/issues">
         <img src="https://img.shields.io/github/issues/mantasjasikenas/flow-launcher-shortcuts-plugin" alt="GitHub issues">
      </a>
      <a href="https://github.com/mantasjasikenas/flow-launcher-shortcuts-plugin/pulls">
         <img src="https://img.shields.io/github/issues-pr/mantasjasikenas/flow-launcher-shortcuts-plugin" alt="GitHub pull requests">
      </a>
      <a href="https://github.com/mantasjasikenas/flow-launcher-shortcuts-plugin/actions/workflows/release.yml">
         <img src="https://img.shields.io/github/actions/workflow/status/mantasjasikenas/flow-launcher-shortcuts-plugin/release.yml?branch=master" alt="GitHub workflow status">
      </a>
      <a href="https://github.com/mantasjasikenas/flow-launcher-shortcuts-plugin/commits">
         <img src="https://img.shields.io/github/last-commit/mantasjasikenas/flow-launcher-shortcuts-plugin" alt="GitHub last commit">
      </a>
   </div>
</div>

> [!WARNING]
> This is a work-in-progress and not the finished product.
>
> Feel free to leave suggestions or report bugs in
> the [issues](https://github.com/mantasjasikenas/flow-launcher-shortcuts-plugin/issues) section.

## Features

Shortcuts plugin features include:

- Multiple shortcut types support
- System and user defined variables support
- Import and export shortcuts
- Duplicate shortcuts
- Open configuration files
- Show list of commands
- Show or set plugin keyword
- Allows to modify configuration files directly

## Settings

The following general options are available on the Flow Launcher settings page.

| Setting             | Description                                                       |
|:--------------------|:------------------------------------------------------------------|
| Activation keyword  | Define the action keyword shortcut to activate plugin             |
| Shortcuts file path | Path to the shortcuts file. If not set, default path will be used |
| Variables file path | Path to the variables file. If not set, default path will be used |

## Usage

The following commands are available for the Shortcuts plugin.

| Command                                                          | Description                | Example                                                                                                                                                               |
|------------------------------------------------------------------|----------------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `` q ``                                                          | Show available commands    | `` q `` to show available commands                                                                                                                                    |
| `` q <shortcut_name> <optional_arguments> ``                     | Run shortcut               | `` q search -q "flow launcher" `` to run shortcut with name `search` and pass `-q "flow launcher"` as arguments. Arguments are optional.                              |
| `` q add <shortcut_type> <shortcut_name> <shortcut_arguments> `` | Add new shortcut           | `` q add directory doc C:\Users\my_user\Documents `` to add a shortcut named `doc` to the `Documents` folder                                                          |
| `` q remove <shortcut_name> ``                                   | Remove shortcut            | `` q remove doc `` to remove shortcut with name `doc`                                                                                                                 |
| `` q var list ``                                                 | Show all variables         | `` q var list `` to show all variables                                                                                                                                |
| `` q var add <variable_name> <variable_value> ``                 | Show or set variable       | `` q var add appdata C:\Users\my_user\AppData\Roaming `` to set value of the `appdata` variable                                                                       |
| `` q var remove <variable_name> ``                               | Remove variable            | `` q var remove appdata `` to remove variable with name `appdata`                                                                                                     |
| `` q keyword get ``                                              | Show plugin action keyword | `` q keyword get `` to show plugin action keyword                                                                                                                     |
| `` q keyword set <keyword> ``                                    | Set plugin keyword         | `` q keyword set ss `` to set plugin keyword to `ss`                                                                                                                  |
| `` q duplicate <shortcut_name> <new_shortcut_name> ``            | Duplicate shortcut         | `` q duplicate doc doc_copy `` to duplicate shortcut with name `doc` to `doc_copy`                                                                                    |
| `` q config ``                                                   | Open configuration files   | `` q config `` to show available configuration files                                                                                                                  |
| `` q reload ``                                                   | Reload configuration files | `` q reload `` to reload configuration files                                                                                                                          |
| `` q import ``                                                   | Import shortcuts           | `` q import `` to import shortcuts from JSON file                                                                                                                     |
| `` q export ``                                                   | Export shortcuts           | `` q export `` to export shortcuts to JSON file                                                                                                                       |
| `` q settings ``                                                 | Open plugin settings       | `` q settings `` to open Flow Launcher settings page                                                                                                                  |
| `` q group list ``                                               | Show all groups            | `` q group list `` to show all groups                                                                                                                                 |
| `` q group add <group_name> <existing_shortcuts_keys> ``         | Add new group              | `` q group add search google bing duckduckgo `` to add a group named `search`` with shortcuts `google`, `bing` and `duckduckgo` These shortcuts should already exist. |
| `` q group remove <group_name> ``                                | Remove group               | `` q group remove search `` to remove group with name `search`                                                                                                        |

## Shortcuts

The following shortcut types are available. More types will be added in the future.

| Shortcut type   | Description                      | Required arguments                      |
|:----------------|:---------------------------------|:----------------------------------------|
| `` directory `` | Open directory in file explorer  | `` directory `` - path to the directory |
| `` file ``      | Open file in default application | `` file `` - path to the file           |
| `` url ``       | Open URL in default browser      | `` url `` - URL to open                 |

## Configuration files

The following configuration files are available.

| Configuration file   | Description                                                    |
|:---------------------|:---------------------------------------------------------------|
| `` shortcuts.json `` | Contains all shortcuts. If not set, default path will be used. |
| `` variables.json `` | Contains all variables. If not set, default path will be used. |

### File structure

#### Shortcuts

If you manually edit the shortcuts file, make sure to reload the plugin for changes to take effect. You can do that by
running `` q reload `` command.

> [!IMPORTANT]
> Make sure that first attribute of the shortcut is the shortcut type.

```json
[
  {
    "Type": "Directory",
    "Path": "C:\\Users",
    "Key": "users"
  },
  {
    "Type": "File",
    "Path": "C:\\Users\\my_user\\Documents\\my_file.txt",
    "Key": "my_file"
  },
  {
    "Type": "Url",
    "Path": "www.google.com",
    "Key": "google"
  },
  {
    "Type": "Group",
    "Shortcuts": [
      {
        "Type": "Url",
        "Url": "www.google.com/search?q=${q}"
      },
      {
        "Type": "Url",
        "Url": "www.bing.com/search?q=${q}"
      },
      {
        "Type": "Url",
        "Url": "www.duckduckgo.com/?q=${q}"
      },
      {
        "Type": "Url",
        "Url": "www.wikipedia.org/wiki/${q}"
      }
    ],
    "Key": "search"
  },
  {
    "Type": "Group",
    "Keys": [
      "meow",
      "g",
      "google"
    ],
    "Key": "multi"
  }
]

```

#### Variables

Variables are used to store values that can be used in shortcuts. For example, you can store your user name and use it.
Variables can be used in shortcuts by using `${variable_name}` syntax. To use system variables, use `%variable_name%`
syntax. Valid variable object attributes are `Name` and `Value`.

```json
[
  {
    "Name": "user",
    "Value": "my_user"
  },
  {
    "Name": "documents",
    "Value": "C:\\Users\\my_user\\Documents"
  }
]
```

## Screenshots

![Commands](assets/screenshots/commands_1.png)
<br/>
<br/>
![Commands](assets/screenshots/commands_2.png)

# Licence

The source code for this plugin is licensed under MIT.