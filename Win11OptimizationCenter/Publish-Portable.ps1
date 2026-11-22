param(
    [string]$OutputDirectory = (Join-Path $PSScriptRoot "..\dist\Win11OptimizationCenter-win-x64"),
    [switch]$CreateZip,
    [switch]$SelfContained
)

$ErrorActionPreference = "Stop"
$project = Join-Path $PSScriptRoot "Win11OptimizationCenter.csproj"
$resolvedOutput = [IO.Path]::GetFullPath($OutputDirectory)
$zipPath = "$resolvedOutput.zip"

if (Test-Path $resolvedOutput) {
    Remove-Item $resolvedOutput -Recurse -Force
}

dotnet publish $project `
    -c Release `
    -r win-x64 `
    --self-contained:$SelfContained `
    -p:WindowsPackageType=None `
    -p:WindowsAppSDKSelfContained=$SelfContained `
    -p:PublishTrimmed=false `
    -p:PublishSingleFile=false `
    -o $resolvedOutput

Copy-Item (Join-Path $PSScriptRoot "..\README.md") (Join-Path $resolvedOutput "README.md")

if ($CreateZip) {
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }

    Compress-Archive -Path (Join-Path $resolvedOutput "*") -DestinationPath $zipPath
    Write-Host "Created: $zipPath"
}

Write-Host "Portable application: $resolvedOutput\Win11OptimizationCenter.exe"
