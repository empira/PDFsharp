# Delete contents of bin and obj folders

#Requires -Version 7
#Requires -PSEdition Core

Push-Location $PSScriptRoot\..\
if (Test-Path .\src\SemVersion.props) {
    Remove-Item .\src\SemVersion.props
    & .\src\foundation\src\shared\src\PdfSharp.BuildConfig\CreateBuildConfiguration.ps1
}
Write-Output "Deleting..."
Get-ChildItem .\ -include bin,obj -Recurse | 
ForEach-Object ($_) {
    Remove-Item $_.fullname -Force -Recurse | Out-Null
}
Write-Output "Done."
Pop-Location
