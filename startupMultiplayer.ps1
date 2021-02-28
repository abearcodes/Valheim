
taskkill /IM "valheim.exe" /F /FI "STATUS eq RUNNING"

Start-Sleep -s 3
Start-Process "E:\Games\SteamLibrary\steamapps\common\ValheimNoSteam\SmartSteamLoader_x64.exe"
Start-Sleep -s 15
Start-Process "E:\Games\SteamLibrary\steamapps\common\ValheimNoSteam2\SmartSteamLoader_x64.exe"