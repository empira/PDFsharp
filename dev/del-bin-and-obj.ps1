# Delete contents of bin and obj folders

#Requires -Version 7
#Requires -PSEdition Core

Push-Location $PSScriptRoot\..\
Get-ChildItem .\ -include bin,obj -Recurse | ForEach-Object ($_) { remove-item $_.fullname -Force -Recurse }
Pop-Location
