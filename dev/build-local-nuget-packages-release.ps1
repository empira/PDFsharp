# Updates local nuget packages.

#Requires -Version 7
#Requires -PSEdition Core

param (
    [Parameter(Mandatory = $false)] [bool]$deleteBinAndObj = $true
)

Push-Location $PSScriptRoot

try {
    Write-Host "Delete bin and obj " $deleteBinAndObj
    if ($deleteBinAndObj) {
        Write-Host "Deleting BIN and OBJ"
        .\del-bin-and-obj.ps1
        Write-Host "Done deleting bin and obj"
    }

    Push-Location ..
    try {
        Write-Host "Invoking ‘dotnet build’"
        dotnet build -c release
        $build = $LASTEXITCODE
        Write-Host "‘dotnet build’ has finished"
    }
    finally {
        Pop-Location
    }

    if ($build -gt 0) {
        Write-Host "‘dotnet build’ failed with code " $build
        throw "‘dotnet build’ failed with code " + $build
    }

    .\update-local-nuget-packages-release.ps1
}
finally {
    Pop-Location
}
