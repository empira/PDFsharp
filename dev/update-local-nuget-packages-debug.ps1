#Requires -Version 7
#Requires -PSEdition Core

Push-Location $PSScriptRoot
./_update-local-nuget-packages.ps1 -config debug -deleteAllPackageVersions $true
Pop-Location
