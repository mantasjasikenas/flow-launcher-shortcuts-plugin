param (
    [string]$userProfileDir = $env:USERPROFILE,
    [bool]$copyToDesktop = $false,
    [string]$pluginName = "Shortcuts",
    [string]$configuration = "Release",
    [bool]$includeDesktopApp = $false
)

# Load functions
. "$PSScriptRoot\utils.ps1"

# Define projects
$shortcutPlugin = "Flow.Launcher.Plugin.ShortcutPlugin"
$shortcutApp = "Flow.Launcher.Plugin.ShortcutPlugin.App"

# Define variables
$parentDirectory = Split-Path -Path $PSScriptRoot -Parent
$pluginsDirectory = Join-Path -Path $userProfileDir -ChildPath "AppData\Roaming\FlowLauncher\Plugins"
$pluginJson = Join-Path -Path $parentDirectory -ChildPath "$shortcutPlugin\plugin.json"
$publishDest = Join-Path -Path $parentDirectory -ChildPath "$shortcutPlugin\bin\Release\win-x64\publish"
$version = Get-PropertyFromJson -jsonFilePath $pluginJson -propertyName "Version"
$pluginDest = Join-Path -Path $pluginsDirectory -ChildPath "$pluginName-$version"
$desktopDest = Join-Path -Path $userProfileDir -ChildPath "Desktop\$pluginName-$version"


Write-Host "Plugin $pluginName-$version publish" -ForegroundColor Magenta
Write-Host
Print-Normal "Script started..."


# Stop Flow Launcher
$process = Get-Process -Name "Flow.Launcher" -ErrorAction SilentlyContinue
if ($process)
{
    Print-Normal "Stopping Flow Launcher..."
    Stop-Process -Name "Flow.Launcher" -Force
    Wait-Process -Name "Flow.Launcher"
}

# Stop Shortcuts
$process = Get-Process -Name "Shortcuts" -ErrorAction SilentlyContinue
if ($process)
{
    Print-Normal "Stopping Shortcuts app..."
    Stop-Process -Name "Shortcuts" -Force
    Wait-Process -Name "Shortcuts"
}


Start-Sleep -Milliseconds 500

# Clean up directories
Print-Normal "Cleaning up directories..."
Remove-Directory -Path $publishDest
Remove-Directory -Path $desktopDest
$directoriesToRemove = Get-ChildItem -Path $pluginsDirectory -Directory | Where-Object { $_.Name -like "$pluginName-*" }
foreach ($directory in $directoriesToRemove)
{
    Remove-Item -Path $directory.FullName -Force -Recurse
}


# Publish plugin
Print-Normal "Building and publishing plugin in $configuration mode..."
$includeEditorParam = if ($includeDesktopApp) { "true" } else { "false" }
dotnet publish $shortcutPlugin -c $configuration -r win-x64 --no-self-contained -p:INCLUDE_EDITOR=$includeEditorParam -o $publishDest

if ($includeDesktopApp)
{
    Print-Normal "Building desktop app..."
    dotnet publish $shortcutApp -c $configuration -r win-x64 -o $publishDest\App /p:Platform=x64
}

$publish_result = $LASTEXITCODE

if ($publish_result -ne 0)
{
    Print-Error "Publish failed with exit code $publish_result"
    exit $publish_result
}

# Copy plugin to destination
Print-Normal "Copying plugin to destination..."
Copy-Item -Path $publishDest -Destination $pluginDest -Force -Recurse


# Start Flow Launcher
Print-Normal "Starting Flow Launcher..."
Start-Process (Join-Path -Path $userProfileDir -ChildPath "AppData\Local\FlowLauncher\Flow.Launcher.exe")


# Process publish to desktop
if ($copyToDesktop)
{
    Print-Normal "Copying plugin to desktop..."
    Copy-Item -Path $publishDest -Destination $desktopDest -Force -Recurse
    Compress-Archive -Path $desktopDest -DestinationPath "$desktopDest.zip" -Force
}

Write-Host
Print-Success "Script finished. Success!"