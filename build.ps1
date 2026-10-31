<#
.SYNOPSIS
    Builds HLDIS in Release and writes the ready-to-install mod ZIP to dist/.

.DESCRIPTION
    The ZIP content (dll, pdb, modinfo.json, modicon.png, assets/) is defined by the PackageMod
    target in HLDIS/HLDIS.csproj. Works with Windows PowerShell 5.1 and PowerShell 7 (Windows, Linux).

.PARAMETER GamePath
    Game folder holding VintagestoryAPI.dll. Defaults to the VINTAGE_STORY environment variable.

.PARAMETER Test
    Also run the Atlas test suite before packaging.

.EXAMPLE
    ./build.ps1 -GamePath "C:\Games\Vintagestory" -Test
#>
param(
    [string]$GamePath = $env:VINTAGE_STORY,
    [switch]$Test
)

$ErrorActionPreference = 'Stop'

function Fail([string]$Message) {
    Write-Error "error: $Message" -ErrorAction Continue
    exit 1
}

if ([string]::IsNullOrWhiteSpace($GamePath)) {
    Fail 'no game folder: pass -GamePath or set VINTAGE_STORY.'
}
if (-not (Test-Path -LiteralPath (Join-Path $GamePath 'VintagestoryAPI.dll'))) {
    Fail "VintagestoryAPI.dll not found in '$GamePath'."
}
# Resolve before changing directory so a relative path keeps working.
$GamePath = (Resolve-Path -LiteralPath $GamePath).Path
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Fail 'dotnet not found: install the .NET 10 SDK.'
}

Push-Location $PSScriptRoot
try {
    if ($Test) {
        Write-Host "==> Running tests against '$GamePath'"
        $previous = $env:VINTAGE_STORY
        $env:VINTAGE_STORY = $GamePath
        try {
            dotnet test HLDIS.Tests/HLDIS.Tests.csproj -nologo
        }
        finally {
            $env:VINTAGE_STORY = $previous
        }
        if ($LASTEXITCODE -ne 0) { Fail 'tests failed.' }
    }

    Write-Host "==> Building mod against '$GamePath'"
    if (Test-Path -LiteralPath 'dist') {
        try { Remove-Item -LiteralPath 'dist' -Recurse -Force }
        catch { Fail 'cannot clean dist/: close any program using the ZIP and retry.' }
    }
    dotnet build HLDIS/HLDIS.csproj -c Release -nologo "-p:VintageStoryPath=$GamePath"
    if ($LASTEXITCODE -ne 0) { Fail 'build failed.' }

    $zip = Get-ChildItem -Path 'dist' -Filter 'hldis_*.zip' -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($null -eq $zip) { Fail 'build finished but no ZIP was written to dist/.' }
    Write-Host "==> Ready: $($zip.FullName)"
}
finally {
    Pop-Location
}
