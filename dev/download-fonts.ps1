Write-Host "Not in use anymore"
Exit 

# Downloads assets.

# This file downloads assets required to compile the PDFsharp/MigraDoc source codes.
# This file runs under PowerShell Core 7.0 or higher.

#Requires -Version 7
#Requires -PSEdition Core

# Download fonts
# https://fonts.google.com/download?family=Noto%20Sans%20TC

$source = "https://fonts.google.com/"
$destination = "$PSScriptRoot/../assets/fonts/Noto/Noto_Sans_TC"

New-Item -ItemType Directory -Path $destination
# New-Item -ItemType Directory -Path "$destination/temp/"

$url = $source + "download?family=Noto%20Sans%20TC"
$dest = $destination

Invoke-WebRequest $url -OutFile "$dest/noto_sans_TC.zip"
Expand-Archive "$destination/noto_sans_TC.zip" -DestinationPath "$destination" -Force

Remove-Item "$destination//noto_sans_TC.zip" -Recurse
# # Successfully extracted. Now move the fonts files.
# [string[]]$folderList = @(
#     "NotoSans"
#     "NotoSans_Condensed"
#     "NotoSans_ExtraCondensed"
#     "NotoSans_SemiCondensed"
# )

# foreach ($folder in $folderList) {
#     Copy-Item -Path "$destination/temp/static/$folder/*" -Include "*.ttf" -Destination $dest
# }

# Remove the folder.
# Remove-Item "$destination/temp" -Recurse
