<#
.SYNOPSIS
Builds iMSIPConfig WPF project (Framework 4.6.2) using full MSBuild.
#>

# --- 1. Find Visual Studio / Build Tools installation ---
$vswhere = "vswhere.exe"

# Find the latest installed VS with MSBuild
$vsInstallPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -property installationPath

if (-not $vsInstallPath) {
    Write-Error "No Visual Studio installation with MSBuild found."
    exit 1
}

$msbuildExe = Join-Path $vsInstallPath "MSBuild\Current\Bin\MSBuild.exe"

if (-not (Test-Path $msbuildExe)) {
    Write-Error "MSBuild.exe not found in $msbuildExe"
    exit 1
}

Write-Host "Please ensure all Nuget packages are restored from the Visual Studio GUI"

# --- 2. Build the solution ---
$solutionPath = ".\iMSIPConfig.sln"
$configuration = "Release"

Write-Host "Building solution..."
& $msbuildExe $solutionPath `
    /t:Clean,Build `
    /p:Configuration=$configuration `
    /p:Platform="Any CPU" `
    /v:m

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed."
    exit $LASTEXITCODE
}

Write-Host "Build completed successfully."