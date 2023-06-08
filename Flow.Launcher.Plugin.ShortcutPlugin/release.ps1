# Variables
$version = "1.0.2"
$publishDest = "C:\Users\tutta\Storage\Dev\Projects\ShortcutPlugin\Flow.Launcher.Plugin.ShortcutPlugin\bin\Release\win-x64\publish"
$pluginsDest = "C:\Users\tutta\AppData\Roaming\FlowLauncher\Plugins\ShortcutManager-$version"
$desktopDest = "C:\Users\tutta\Desktop\ShortcutManager-$version"

$saveToDesktop = $false

taskkill /im Flow.Launcher.exe /F
dotnet publish Flow.Launcher.Plugin.ShortcutPlugin -c Release -r win-x64 --no-self-contained -o $publishDest
if (Test-Path $pluginsDest)
{
    Remove-Item -Path $pluginsDest -Force -Recurse
}
Copy-Item -Path $publishDest -Destination $pluginsDest -Force -Recurse
Copy-Item -Path "C:\Users\tutta\Storage\Dev\Resources\shortcuts.json" -Destination "$pluginsDest\Config" -Force

# Processing publish to desktop
if ($saveToDesktop)
{
    if (Test-Path $desktopDest)
    {
        Remove-Item -Path $desktopDest -Force -Recurse
    }

    Copy-Item -Path $publishDest -Destination $desktopDest -Force -Recurse
    Compress-Archive -Path $desktopDest -DestinationPath "$desktopDest.zip" -Force
}


Start-Process C:\Users\tutta\AppData\Local\FlowLauncher\Flow.Launcher.exe