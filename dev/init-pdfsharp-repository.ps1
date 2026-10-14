# Initialize PDFsharp repository after a fresh git clone.

#Requires -Version 7
#Requires -PSEdition Core

Push-Location $PSScriptRoot/..

# Download latest assets.  TODO
# ./dev/download-assets.ps1
 ./dev/del-bin-and-obj.ps1

# Create correct git version information by producing
# SemVersion.props and SemVersionInformation-generated.cs.
dotnet build .\src\foundation\src\shared\src\PdfSharp.GitVersion\PdfSharp.GitVersion.csproj

# Build the first time DEBUG and RELEASE
# Compile for testing
dotnet build 
dotnet build --configuration release

Pop-Location
