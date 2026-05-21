#Requires -Version 5.1
<#
.SYNOPSIS
    Packs UniLocalizer and copies the package to the local NuGet feed.

.EXAMPLE
    .\publish-nuget-packages.ps1

.EXAMPLE
    .\publish-nuget-packages.ps1 -Configuration Debug

.EXAMPLE
    .\publish-nuget-packages.ps1 -SkipVersionConfirm
#>
[CmdletBinding()]
param(
    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Release',

    [string] $FeedPath = '\\nas-01\IT\Projekty\_NuGet_Storage',

    [switch] $SkipVersionConfirm
)

$ErrorActionPreference = 'Stop'

$repoRoot = $PSScriptRoot
$artifactsDir = Join-Path $repoRoot 'artifacts\nuget-packages'

$project = Join-Path $repoRoot 'UniLocalizer\UniLocalizer.csproj'

function Write-Step([string] $Message) {
    Write-Host "==> $Message" -ForegroundColor Cyan
}

function Get-ProjectPackageVersion([string] $ProjectPath) {
    [xml] $csproj = Get-Content -LiteralPath $ProjectPath
    foreach ($group in $csproj.Project.PropertyGroup) {
        if (-not $group.Version) { continue }
        $version = $group.Version
        if ($version -is [System.Xml.XmlElement]) {
            return $version.InnerText.Trim()
        }
        return [string] $version.Trim()
    }
    return '(Version not set in csproj)'
}

function Confirm-PackageVersionUpdated([string[]] $ProjectPaths) {
    Write-Host ''
    Write-Host 'Package version check' -ForegroundColor Yellow
    foreach ($projectPath in $ProjectPaths) {
        $relative = $projectPath.Substring($repoRoot.Length).TrimStart('\', '/')
        $version = Get-ProjectPackageVersion -ProjectPath $projectPath
        Write-Host "  $relative : $version"
    }
    Write-Host ''
    Write-Host 'Did you increase the version in the .csproj file(s) when behavior or public API meaning changed?' -ForegroundColor Yellow
    Write-Host '  [Y] Yes — continue publish' -ForegroundColor DarkGray
    Write-Host '  [N] No  — cancel (bump <Version> in csproj first)' -ForegroundColor DarkGray

    do {
        $answer = (Read-Host 'Continue').Trim()
        if ($answer -eq '') { $answer = 'Y' }
    } while ($answer -notin @('Y', 'y', 'N', 'n'))

    if ($answer -in @('N', 'n')) {
        Write-Host 'Publish cancelled. Update package version(s), then run this script again.' -ForegroundColor Yellow
        exit 1
    }
}

Write-Step "Repository: $repoRoot"
Write-Step "Configuration: $Configuration"
Write-Step "NuGet feed: $FeedPath"

if (-not $SkipVersionConfirm) {
    Confirm-PackageVersionUpdated -ProjectPaths @($project)
}

if (-not (Test-Path -LiteralPath $FeedPath)) {
    throw "NuGet feed path is not reachable: $FeedPath"
}

if (Test-Path -LiteralPath $artifactsDir) {
    Write-Step "Cleaning $artifactsDir"
    Get-ChildItem -LiteralPath $artifactsDir -Force -ErrorAction SilentlyContinue |
        Remove-Item -Recurse -Force -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Path $artifactsDir -Force | Out-Null

Write-Step 'Restoring and building project (XML docs required for pack)'
dotnet restore $project
dotnet build $project -c $Configuration --no-restore

Write-Step 'Packing UniLocalizer'
dotnet pack $project -c $Configuration --no-restore --no-build -o $artifactsDir

$packages = Get-ChildItem -Path $artifactsDir -Filter '*.nupkg' -File | Sort-Object Name
if ($packages.Count -eq 0) {
    throw "No .nupkg files were produced in $artifactsDir"
}

Write-Step 'Copying packages to feed'
foreach ($package in $packages) {
    $destination = Join-Path $FeedPath $package.Name
    Copy-Item -LiteralPath $package.FullName -Destination $destination -Force
    Write-Host "  Published $($package.Name)" -ForegroundColor Green
}

Write-Host ''
Write-Host 'Done. Published packages:' -ForegroundColor Green
$packages | ForEach-Object { Write-Host "  - $($_.Name)" }
