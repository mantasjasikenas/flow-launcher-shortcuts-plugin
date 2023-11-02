param (
    [string]$userProfileDir = $env:USERPROFILE,
    [bool]$copyToDesktop = $false,
    [string]$pluginName = "Shortcuts"
)

function Get-PropertyFromJson
{
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

function Remove-Directory($path)
{
    if (Test-Path $path)
    {
        Remove-Item -Force -Recurse -Path "$path\*"
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


Write-Host "Plugin $pluginName-$version publish" -ForegroundColor Magenta
Write-Host
Write-Host "Script started..." -ForegroundColor Yellow


# Stop Flow Launcher
Write-Host "Stopping Flow Launcher..." -ForegroundColor Yellow
Stop-Process -Name "Flow.Launcher" -Force


# Clean up directories
Write-Host "Cleaning up directories..." -ForegroundColor Yellow
Remove-Directory -Path $publishDest
Remove-Directory -Path $desktopDest
$directoriesToRemove = Get-ChildItem -Path $pluginsDirectory -Directory | Where-Object { $_.Name -like "$pluginName-*" }
foreach ($directory in $directoriesToRemove)
{
    Remove-Item -Path $directory.FullName -Force -Recurse
}


# Publish plugin
Write-Host "Building and publishing plugin..." -ForegroundColor Yellow
$publish = dotnet publish "Flow.Launcher.Plugin.ShortcutPlugin" -c Release -r win-x64 --no-self-contained -o $publishDest
$publish_result = $LASTEXITCODE

if ($publish_result -ne 0)
{
    Write-Host "Publish failed with exit code $publish_result" -ForegroundColor Red
    exit $publish_result
}

# Copy plugin to destination
Write-Host "Copying plugin to destination..." -ForegroundColor Yellow
Copy-Item -Path $publishDest -Destination $pluginDest -Force -Recurse


# Start Flow Launcher
Write-Host "Starting Flow Launcher..." -ForegroundColor Yellow
Start-Process (Join-Path -Path $userProfileDir -ChildPath "AppData\Local\FlowLauncher\Flow.Launcher.exe")


# Process publish to desktop
if ($copyToDesktop)
{
    Write-Host "Copying plugin to desktop..." -ForegroundColor Yellow
    Copy-Item -Path $publishDest -Destination $desktopDest -Force -Recurse
    Compress-Archive -Path $desktopDest -DestinationPath "$desktopDest.zip" -Force
}

Write-Host
Write-Host "Script finished. Success!" -ForegroundColor Green