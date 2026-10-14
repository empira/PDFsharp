# DELETE

# Use GitVersion (https://gitversion.net/) to create all information we need during


# Wait until the post-build step of PdfSharp.GitVersion creates SemVersionInformation-generated.cs.
# Works fine with visual studio, but not with MS-BUILD (dotnet build).
# For me (StL) it is unclear why.

# Write-Host "Check existence of SemVersionInformation-generated.cs."
#while (-not (Test-Path "Internal/SemVersionInformation-generated.cs")) {
#	Write-Host "Wait for generation of SemVersionInformation-generated.cs."
#	Start-Sleep -Milliseconds 500
#} 
