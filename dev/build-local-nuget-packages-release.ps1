<#
.SYNOPSIS
    Build PDFsharp with config release and updates the local nuget packages.

.DESCRIPTION
    The script deletes all artifacts and builds a PDFsharp release version.
    It first builds the PdfSharp.BuildConfig.csproj to ensure that SemVersion.props and 
    PDFsharpBuildConfig.props are up-to-date before the build process starts.
    The created packages are copied to two locations.
    The first is the .nuget-local directory in your user profile directory.
    The second is the .nuget directory in the root folder of your project in case
    you used PDFsharp as a submodule.
#>

#Requires -Version 7
#Requires -PSEdition Core

param (
    [Parameter(Mandatory = $false)] [bool]$deleteBinAndObj = $true
)

Push-Location $PSScriptRoot

try {    
    # Write-Output "Delete bin and obj " $deleteBinAndObj
    if ($deleteBinAndObj) {
        Write-Output "Deleting ‘/bin’ and ‘/obj’..."
        .\del-bin-and-obj.ps1 | Out-Null
        Write-Output "Done."
    }

    Push-Location ..
    try {
        Write-Output "Invoking ‘dotnet build’ for ‘PdfSharp.BuildConfig.csproj’"
        # Generate semver infos and PDFsharp build configuration first.
        dotnet build .\src\foundation\src\shared\src\PdfSharp.BuildConfig\PdfSharp.BuildConfig.csproj
        Write-Output "Invoking ‘dotnet build’ for PDFsharp solution"
        dotnet build --configuration release
        $build = $LASTEXITCODE
        Write-Output "‘dotnet build’ has finished"
    }
    finally {
        Pop-Location
    }

    if ($build -gt 0) {
        Write-Error "‘dotnet build’ failed with code " $build
        throw "‘dotnet build’ failed with code " + $build
    }

     Write-Output "Invoking ‘update-local-nuget-packages-release.ps1’"
     .\update-local-nuget-packages-release.ps1
}
finally {
    Pop-Location
}
