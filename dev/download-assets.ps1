# Downloads assets.

# This file downloads assets required to compile the PDFsharp/MigraDoc source codes.
# This file runs under PowerShell Core 7.0 or higher.

#Requires -Version 7
#Requires -PSEdition Core

# Get-ChildItem .\ -include bin,obj -Recurse | ForEach-Object ($_) { remove-item $_.fullname -Force -Recurse }

# Part 1: Download assets from assets.pdfsharp.net

[string[]]$assetList = @(
    "pdfsharp/pdfsharp.zip"
    "migradoc/migradoc.zip"
    "pdfsharp-6.x/pdfsharp-6.x.zip"
    # Grammar by example archive => Move to PDFsharp.Lab
    "archives/grammar-by-example/GBE.zip"
    "archives/samples-1.5/samples-1.5.zip"
)

$source = "https://assets.pdfsharp.net/"
$destination = "$PSScriptRoot/../assets/"

if (test-path -PathType container $destination) {
    Remove-Item -LiteralPath $destination -Force -Recurse
}
New-Item -ItemType Directory -Path $destination

# Download assets version.
$url = $source + ".assets-version"
$dest = $destination + ".assets-version"
Invoke-WebRequest $url -OutFile $dest

# Download assets files.
foreach ($asset in $assetList) {
    $url = $source + $asset
    $dest = $destination + $asset

    $folder = [IO.Path]::GetDirectoryName($dest)
    New-Item -ItemType Directory -Path $folder -Force

    Invoke-WebRequest $url -OutFile $dest

    $idx = $asset.LastIndexOf("/")
    $assetFolder = $asset.Substring(0, $idx)
    Expand-Archive -Force "$destination/$asset" -DestinationPath "$destination/$assetFolder"

    Remove-Item "$destination/$asset"
    # Not all ZIP files contain compress.ps1. Suppress error messages.
    Remove-Item "$destination/$assetFolder/compress.ps1" -ErrorAction Ignore
}

# Part 2: Download fonts

$source = "https://assets.pdfsharp.net/"
$destination = "$PSScriptRoot/../assets/fonts/Noto/Noto_Sans/static/"

New-Item -ItemType Directory -Path $destination
New-Item -ItemType Directory -Path "$destination/temp/"

$url = $source + "fonts/Noto/Noto_Sans.zip"
$dest = $destination

Invoke-WebRequest $url -OutFile "$dest/temp/noto_sans_temp.zip"
Expand-Archive -Force "$destination/temp/noto_sans_temp.zip" -DestinationPath "$destination/temp/"

# Successfully extracted. Now move the fonts files.
[string[]]$folderList = @(
    "NotoSans"
    "NotoSans_Condensed"
    "NotoSans_ExtraCondensed"
    "NotoSans_SemiCondensed"
)

foreach ($folder in $folderList) {
    Copy-Item -Path "$destination/temp/static/$folder/*" -Include "*.ttf" -Destination $dest
}

# Remove the folder.
Remove-Item "$destination/temp" -Recurse
