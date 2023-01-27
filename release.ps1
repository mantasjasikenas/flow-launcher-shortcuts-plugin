taskkill /im Flow.Launcher.exe /F
dotnet publish Flow.Launcher.Plugin.ShortcutPlugin -c Release -r win-x64 --no-self-contained
Copy-Item "C:\Users\tutta\OneDrive - Kaunas University of Technology\PERSONAL\Code\Projects\ShortcutPlugin\Flow.Launcher.Plugin.ShortcutPlugin\bin\Release\win-x64\publish/*" "C:\Users\tutta\AppData\Roaming\FlowLauncher\Plugins\ShortcutManager" -Recurse -Force
Start-Process C:\Users\tutta\AppData\Local\FlowLauncher\Flow.Launcher.exe
