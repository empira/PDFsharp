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
Push-Location $PSScriptRoot
Push-Location .\..

Write-Host "Copy all `e[93m$($config.ToUpperInvariant())$r NuGet packages to $b$nugetLocal$r."
New-Item -Path $nugetLocal -ItemType directory -Force | Out-Null
$packages = @()
Get-ChildItem -Path . -Filter *.nupkg -Recurse -ErrorAction SilentlyContinue -Force | ForEach-Object {
    if ($_.FullName -match "bin\\$config|bin\/$config") {
        Copy-Item $_.FullName -Destination ("$nugetLocal\" + $_.Name)
        $packages += $_.Name
    }
}
Write-Host "  $b$($packages.Count)$r packages copied."

Write-Host "Delete all existing old package folders in $b$nuget$r folder."
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
Write-Host "  $b$count$r package $($deleteAllPackageVersions ? '' : 'version ')folders deleted in $b$nuget$r."
if ($packages.Count -gt 0) {
    Write-Host "  New version number(s) : $b$Versions$r"
}

Pop-Location
Pop-Location
