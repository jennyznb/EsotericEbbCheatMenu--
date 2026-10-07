[CmdletBinding()]
param(
    [string]$GameDir = "C:\Program Files (x86)\Steam\steamapps\common\Esoteric Ebb",
    [string]$OutputDir = $PSScriptRoot,
    [switch]$Deploy
)

$ErrorActionPreference = "Stop"

function Find-Csc {
    $sdkRoot = "C:\Program Files\dotnet\sdk"
    if (-not (Test-Path -LiteralPath $sdkRoot)) {
        throw ".NET SDK not found at $sdkRoot"
    }

    $sdk = Get-ChildItem -LiteralPath $sdkRoot -Directory |
        Where-Object {
            Test-Path -LiteralPath (Join-Path $_.FullName "Roslyn\bincore\csc.dll")
        } |
        Sort-Object { [version]$_.Name } -Descending |
        Select-Object -First 1

    if (-not $sdk) {
        throw "csc.dll not found in any installed .NET SDK."
    }

    return Join-Path $sdk.FullName "Roslyn\bincore\csc.dll"
}

function Find-Net6RefDir {
    $refRoot = "C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref"
    if (-not (Test-Path -LiteralPath $refRoot)) {
        throw ".NET reference packs not found at $refRoot"
    }

    $refPack = Get-ChildItem -LiteralPath $refRoot -Directory |
        Where-Object { $_.Name -like "6.0.*" } |
        Sort-Object { [version]$_.Name } -Descending |
        Select-Object -First 1

    if (-not $refPack) {
        throw ".NET 6 reference pack not found."
    }

    $refDir = Join-Path $refPack.FullName "ref\net6.0"
    if (-not (Test-Path -LiteralPath $refDir)) {
        throw "Reference directory not found: $refDir"
    }

    return $refDir
}

$csc = Find-Csc
$refDir = Find-Net6RefDir

$src = Join-Path $PSScriptRoot "CheatMenu.cs"
if (-not (Test-Path -LiteralPath $src)) {
    throw "Source not found: $src"
}

$out = Join-Path $OutputDir "EsotericEbbCheatMenu.dll"
New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null

$gameRefs = @(
    "$GameDir\BepInEx\interop\Il2Cppmscorlib.dll",
    "$GameDir\BepInEx\interop\Il2CppSystem.dll",
    "$GameDir\BepInEx\interop\Il2CppSystem.Core.dll",
    "$GameDir\BepInEx\core\BepInEx.Core.dll",
    "$GameDir\BepInEx\core\BepInEx.Unity.IL2CPP.dll",
    "$GameDir\BepInEx\core\Il2CppInterop.Runtime.dll",
    "$GameDir\BepInEx\core\Il2CppInterop.Common.dll",
    "$GameDir\BepInEx\core\0Harmony.dll",
    "$GameDir\BepInEx\interop\UnityEngine.CoreModule.dll",
    "$GameDir\BepInEx\interop\UnityEngine.IMGUIModule.dll",
    "$GameDir\BepInEx\interop\UnityEngine.InputLegacyModule.dll",
    "$GameDir\BepInEx\interop\UnityEngine.TextRenderingModule.dll",
    "$GameDir\BepInEx\interop\UnityEngine.UIModule.dll",
    "$GameDir\BepInEx\interop\Unity.InputSystem.dll",
    "$GameDir\BepInEx\interop\Assembly-CSharp.dll"
)

foreach ($reference in $gameRefs) {
    if (-not (Test-Path -LiteralPath $reference)) {
        throw "Missing game reference: $reference"
    }
}

$cscArgs = New-Object System.Collections.Generic.List[string]
foreach ($argument in @(
    $csc,
    "/nologo",
    "/target:library",
    "/nostdlib+",
    "/langversion:latest",
    "/optimize+",
    "/nowarn:0433,1685",
    "/out:$out"
)) {
    $cscArgs.Add($argument)
}

$cscArgs.Add("/reference:$refDir\System.Runtime.dll")
Get-ChildItem -LiteralPath $refDir -Filter *.dll |
    Where-Object Name -ne "System.Runtime.dll" |
    ForEach-Object { $cscArgs.Add("/reference:$($_.FullName)") }

foreach ($reference in $gameRefs) {
    $cscArgs.Add("/reference:$reference")
}

$cscArgs.Add($src)

& dotnet $cscArgs.ToArray()
if ($LASTEXITCODE -ne 0) {
    throw "Compile failed with exit code $LASTEXITCODE"
}

Write-Host "Built: $out"

if ($Deploy) {
    $pluginDir = Join-Path $GameDir "BepInEx\plugins\EsotericEbbCheatMenu"
    New-Item -ItemType Directory -Path $pluginDir -Force | Out-Null
    Copy-Item -LiteralPath $out -Destination (Join-Path $pluginDir "EsotericEbbCheatMenu.dll") -Force
    Write-Host "Deployed: $pluginDir\EsotericEbbCheatMenu.dll"
}
