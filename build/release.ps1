param (
    [string]$userProfileDir = $env:USERPROFILE,
    [bool]$copyToDesktop = $false,
    [string]$pluginName = "ShortcutManager"
)

function Get-PropertyFromJson {
    param (
        [string]$jsonFilePath,
        [string]$propertyName
    )

    # Read the JSON file content
    $jsonContent = Get-Content -Path $pluginJson -Raw

    # Parse the JSON content
    $jsonObject = ConvertFrom-Json -InputObject $jsonContent

    # Return the extracted property
    return $jsonObject.$propertyName
}

function Remove-Directory($path) {
    if (Test-Path $path) {
        Remove-Item -Path $path -Force -Recurse
    }
}


# Define variables
$parentDirectory = Split-Path -Path $PSScriptRoot -Parent
$pluginsDirectory = Join-Path -Path $userProfileDir -ChildPath "AppData\Roaming\FlowLauncher\Plugins"
$pluginJson = Join-Path -Path $parentDirectory -ChildPath "Flow.Launcher.Plugin.ShortcutPlugin\plugin.json"
$publishDest = Join-Path -Path $parentDirectory -ChildPath "Flow.Launcher.Plugin.ShortcutPlugin\bin\Release\win-x64\publish"
$version = Get-PropertyFromJson -jsonFilePath $pluginJson -propertyName "Version"
$pluginDest = Join-Path -Path $pluginsDirectory -ChildPath "$pluginName-$version"
$desktopDest = Join-Path -Path $userProfileDir -ChildPath "Desktop\$pluginName-$version"


# Stop Flow Launcher
Stop-Process -Name "Flow.Launcher" -Force


# Clean up directories
Remove-Directory -Path $publishDest
Remove-Directory -Path $desktopDest
$directoriesToRemove = Get-ChildItem -Path $pluginsDirectory -Directory | Where-Object { $_.Name -like "ShortcutManager*" }
foreach ($directory in $directoriesToRemove) {
    Remove-Item -Path $directory.FullName -Force -Recurse
}


# Publish plugin
dotnet publish "Flow.Launcher.Plugin.ShortcutPlugin" -c Release -r win-x64 --no-self-contained -o $publishDest


# Copy plugin to destination
Copy-Item -Path $publishDest -Destination $pluginDest -Force -Recurse


# Start Flow Launcher
Start-Process (Join-Path -Path $userProfileDir -ChildPath "AppData\Local\FlowLauncher\Flow.Launcher.exe")


# Process publish to desktop
if ($copyToDesktop) {
    Copy-Item -Path $publishDest -Destination $desktopDest -Force -Recurse
    Compress-Archive -Path $desktopDest -DestinationPath "$desktopDest.zip" -Force
}