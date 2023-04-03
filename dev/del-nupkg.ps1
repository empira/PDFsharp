#Requires -Version 7
#Requires -PSEdition Core

Get-ChildItem .\ -Include *.nupkg -Recurse | ForEach-Object ($_) { Remove-Item $_.FullName -Force -Recurse }
