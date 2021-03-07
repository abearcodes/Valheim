
$dlls = @(
    ".\ABearCodes.Valheim.SimpleAutoGameStart\bin\Debug\ABearCodes.Valheim.SimpleAutoGameStart.dll",
    ".\ABearCodes.Valheim.CraftingWithContainers\bin\Debug\ABearCodes.Valheim.CraftingWithContainers.dll"
#    ".\ABearCodes.Valheim.SimpleRecycling\bin\Debug\ABearCodes.Valheim.SimpleRecycling.dll"
)

$targets = @(
    "E:\Games\SteamLibrary\steamapps\common\Valheim\BepInEx\plugins",
    "E:\Games\SteamLibrary\steamapps\common\ValheimNoSteam\BepInEx\plugins",
    "E:\Games\SteamLibrary\steamapps\common\ValheimNoSteam2\BepInEx\plugins"
)


taskkill /IM "valheim.exe" /F /FI "STATUS eq RUNNING"
Start-Sleep -s 1


foreach ($dll in $dlls){
    foreach($target in $targets){
        copy $dll $target
    }
}