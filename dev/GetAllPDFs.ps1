# Chat/GPT creates this script for me. That is why comments are German.
# param(
#     [Parameter(Mandatory = $true)]
#     [string]$SourcePath,

#     [Parameter(Mandatory = $true)]
#     [string]$DestinationPath
# )

TODO verschieben

$SourcePath = "D:\repos\emp\PDFsharp.iText\itext.tests"
$DestinationPath = "D:\repos\emp\PDFsharp\assets\private\test-files"

# Zielverzeichnis erstellen, falls es nicht existiert
if (-not (Test-Path -Path $DestinationPath)) {
    New-Item -ItemType Directory -Path $DestinationPath | Out-Null
}

# Alle PDF-Dateien rekursiv suchen
$files = Get-ChildItem -Path $SourcePath -Recurse -Filter *.pdf

foreach ($file in $files) {
    # Dateiname ermitteln
    $destFile = Join-Path $DestinationPath $file.Name

    # Falls Datei schon existiert → eindeutigen Namen erzeugen
    if (Test-Path $destFile) {
        $baseName = [System.IO.Path]::GetFileNameWithoutExtension($file.Name)
        $extension = $file.Extension
        $counter = 1
        do {
            $newName = "$baseName`_$counter$extension"
            $destFile = Join-Path $DestinationPath $newName
            $counter++
        } while (Test-Path $destFile)
    }

    # Datei kopieren
    Copy-Item -Path $file.FullName -Destination $destFile
}
