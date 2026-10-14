# Compresses all files of PDFsharp into a PDFsharp.zip file

#Requires -Version 7
#Requires -PSEdition Core

Push-Location "$PSScriptRoot/.."

try {
    # $root = "$PSScriptRoot/.."
    Remove-Item zip -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item PDFsharp.zip -Force -ErrorAction SilentlyContinue
    # New-Item -ItemType Directory -Force -Path $Root/PDFsharp.zip
    New-Item -ItemType Directory -Force -Path zip

    # I tried an hour or so to do it with Get-Childitem. No way, I finally use robocopy...
    # robocopy . $root/zip *.* /dcopy:t /e /xa:h /xd .git /xd "bin" /xd "obj" /xd assets /xd zip
    robocopy . zip/PDFsharp *.* /dcopy:t /e /xa:h /xd .git /xd .vs /xd .vscode /xd "bin" /xd "obj" /xd assets /xd zip `
    /xd TestResults /xd localtests `
    /xf debugSettings.json /xf *.user /xf *.userprefs /xf launchsettings.json /xf testEnvironments.json
    # robocopy from to *.* /dcopy:t /e /xa:h /xd "System Volume Information" /xd "$RECYCLE*"

    # Delete generated files
    Remove-Item zip/PDFsharp/src/SemVersion.props                                                                       -ErrorAction SilentlyContinue
    Remove-Item zip/PDFsharp/src/PdfSharpBuildConfig.props                                                              -ErrorAction SilentlyContinue
    Remove-Item zip/PDFsharp/src/foundation/src/shared/src/PdfSharp.Shared/Internal/SSemVersionInformation-generated.cs -ErrorAction SilentlyContinue

    # Get-ChildItem zip -Recurse | measure

    Push-Location "zip"
    Compress-Archive -Path PDFsharp -DestinationPath ../PDFsharp.zip -CompressionLevel Fastest -Force
    Pop-Location

    # Remove-Item $root/zip -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item zip -Recurse -Force -ErrorAction SilentlyContinue
}
finally {
    Pop-Location
}
