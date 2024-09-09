param (
    [string]$userProfileDir = $env:USERPROFILE,
    [bool]$copyToDesktop = $false,
    [string]$pluginName = "Shortcuts",
    [string]$configuration = "Release"
)

# Load functions
. "$PSScriptRoot\utils.ps1"

# Define variables
$parentDirectory = Split-Path -Path $PSScriptRoot -Parent
$pluginsDirectory = Join-Path -Path $userProfileDir -ChildPath "AppData\Roaming\FlowLauncher\Plugins"
$pluginJson = Join-Path -Path $parentDirectory -ChildPath "Flow.Launcher.Plugin.ShortcutPlugin\plugin.json"
$publishDest = Join-Path -Path $parentDirectory -ChildPath "Flow.Launcher.Plugin.ShortcutPlugin\bin\Release\win-x64\publish"
$version = Get-PropertyFromJson -jsonFilePath $pluginJson -propertyName "Version"
$pluginDest = Join-Path -Path $pluginsDirectory -ChildPath "$pluginName-$version"
$desktopDest = Join-Path -Path $userProfileDir -ChildPath "Desktop\$pluginName-$version"


Write-Host "Plugin $pluginName-$version publish" -ForegroundColor Magenta
Write-Host
Print-Normal "Script started..."


# Stop Flow Launcher
Print-Normal "Stopping Flow Launcher..."
Stop-Process -Name "Flow.Launcher" -Force
Wait-Process -Name "Flow.Launcher"

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
if ($configuration -eq "Debug")
{
    Print-Normal "Building and publishing plugin in Debug mode..."
    $publish = dotnet publish "Flow.Launcher.Plugin.ShortcutPlugin" -c Debug -r win-x64 --no-self-contained -o $publishDest
}
else
{
    Print-Normal "Building and publishing plugin in Release mode..."
    $publish = dotnet publish "Flow.Launcher.Plugin.ShortcutPlugin" -c Release -r win-x64 --no-self-contained -o $publishDest
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