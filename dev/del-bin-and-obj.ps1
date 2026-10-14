# Delete contents of bin and obj folders

#Requires -Version 7
#Requires -PSEdition Core

Push-Location $PSScriptRoot\..\
# Only delete bin and obj folders inside the src folder, so assets are not affected.
Get-ChildItem .\src -include bin,obj -Recurse | ForEach-Object ($_) { remove-item $_.fullname -Force -Recurse }
Pop-Location
