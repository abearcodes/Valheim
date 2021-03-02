$dllPath = ".\bin\Release\ABearCodes.Valheim.SimpleRecycling.dll"
$thunderStoreResourcesDirPath = ".\ThunderStoreResources\"
$targetDirPath = ".\releases\"

$manifestPath = "$($thunderStoreResourcesDirPath)manifest.json"
$iconPath = "$($thunderStoreResourcesDirPath)icon.png"
$readmePath = "$($thunderStoreResourcesDirPath)README.md"

$dllRef = ls $dllPath
$dllName = $dllRef | % {$_.BaseName}
$version = $dllRef | % { $_.versioninfo.fileversion} 
Write-Host "Releasing version $version"

$manifest = Get-Content $manifestPath | ConvertFrom-Json
$manifest.version_number=$version
$manifest | ConvertTo-Json | Set-Content $manifestPath

mkdir $targetDirPath -ErrorAction SilentlyContinue

$compress = @{
    Path = $dllPath, $manifestPath, $iconPath, $readmePath
    CompressionLevel = "Optimal"
    DestinationPath = "$($targetDirPath)$($dllName).v$($version).zip"
}
Compress-Archive @compress

$compress = @{
    Path = $dllPath
    CompressionLevel = "Optimal"
    DestinationPath = "$($targetDirPath)$($dllName).v$($version).Nexus.zip"
}
Compress-Archive @compress

