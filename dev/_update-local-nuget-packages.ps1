# Updates local nuget packages - implementation

#Requires -Version 7
#Requires -PSEdition Core

param (
    [Parameter(Mandatory = $false)] [string]$config = 'release',
    [Parameter(Mandatory = $false)] [bool]$deleteAllPackageVersions = $true
)

$userprofile = $env:USERPROFILE # Windows
if (!$userprofile) {
    $userprofile = $env:HOME # Linux
}
if (!$userprofile) {
    throw "Could not locate user directory."
}

$nuget = "$userprofile\.nuget\packages"
$nugetLocal = "$userprofile\.nuget-local"
$b = "`e[94m"
$r = "`e[0m"

# Set working directory to solution root.
Push-Location $PSScriptRoot\..

# Machine local NuGet packages
# Do not copy packages into user profile folder NugetFolder
# if .nuget-local does not exist.
if (Test-Path -Path $nugetLocal) {
    # User has a local NuGet directory.
} else {
    $nugetLocal = ""
}

# Project local NuGet packages (if PDFsharp is used as submodule)
# Do not copy packages into .nuget directory if PDFsharp is not used
# as a submodule and therefore .nuget does not exist in project directory.
$nugetProject = "..\..\.nuget"  # because of this: YOUR-PROJECT/modules/PDFsharp
if (Test-Path -Path $nugetProject) {
    # PDFsharp is a submodule and the outer project has a .nuget directory.
    $nugetProject = $nugetProject
} else {
    $nugetProject = ""
}

$hasNugetLocal = $nugetLocal.Length -gt 0
$hasNugetProject = $nugetProject.Length -gt 0

if ($hasNugetLocal -or $hasNugetProject)
{
    if ($hasNugetLocal -and $hasNugetProject)
    {
        $targetFolders = "$b$nugetLocal$r and $b$nugetProject$r"
    }
    elseif ($hasNugetLocal)
    {
        $targetFolders = "$b$nugetLocal$r"
    }
    elseif ($hasNugetProject)
    {
        $targetFolders = "$b$nugetProject$r"
    }

    Write-Output "Copy all `e[93m$($config.ToUpperInvariant())$r NuGet packages to $targetFolders."
    # Do not force creation of directory anymore.
    # New-Item -Path $nugetLocal -ItemType directory -Force | Out-Null
    $packages = @()
    Get-ChildItem -Path . -Filter *.nupkg -Recurse -ErrorAction SilentlyContinue -Force | ForEach-Object {
        if ($_.FullName -match "bin\\$config|bin\/$config") {
            if ($nugetLocal.Length -gt 0) {
                Copy-Item $_.FullName -Destination ("$nugetLocal\" + $_.Name)
            }
            if ($nugetProject.Length -gt 0) {
                Copy-Item $_.FullName -Destination ("$nugetProject\" + $_.Name)
            }
            $packages += $_.Name
        }
    }
    Write-Output "  $b$($packages.Count)$r packages copied."
}
else
{
    Write-Output "No `e[93m$($config.ToUpperInvariant())$r NuGet packages are copied as there is no .nuget-local and no main module .nuget folder."
}


Write-Output "Delete all existing old package folders in $b$nuget$r folder."
$count = 0
$versions = @()
$packages | ForEach-Object {
    $package = ($_ -split "\.\d")[0]
    $version = $_.Substring($package.Length + 1)
    $version = $version.Substring(0, $version.IndexOf(".nupkg"))
    if (!$versions.Contains($version)) {
            $versions += $version
    }
    Get-ChildItem -Path $nuget -ErrorAction SilentlyContinue -Force | ForEach-Object {
        if ($_.Name -eq $package) {
            if ($deleteAllPackageVersions) {
                Remove-Item -Path $_.FullName -Recurse -Force   
                $count++
            }
            else {
                Get-ChildItem -Path "$nuget\$package" -ErrorAction SilentlyContinue -Force | ForEach-Object {
                    if ($_.Name -eq $version) {
                        Remove-Item -Path $_.FullName -Recurse -Force   
                        $count++
                    }
                }
            }
        }
    }
}
Write-Output "  $b$count$r package $($deleteAllPackageVersions ? '' : 'version ')folders deleted in $b$nuget$r."
if ($packages.Count -gt 0) {
    # In case you build several times and do not delete the artifacts
    # you can have more than one set of NuGet packages 
    Write-Output "  The new version number(s) are: $b$Versions$r"
}

Pop-Location
