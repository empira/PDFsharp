# dev folder

## del-bin-and-obj
Deletes the content of all `bin` and `obj` folders.
Theoretically not needed but practically very useful if strange behaviors happen using `dotnet build`.

## download-assets
Downloads all assets like images, fonts, and PDF files to the assets folder in this repository.

## update-local-nuget-packages-debug / update-local-nuget-packages-release
Copies all debug or release NuGet packages to the users local nuget package folder.

## build-local-nuget-packages-release
Builds NuGet packages for release on NuGet.org.

## run-tests
Run unit tests on Windows and WSL2.
