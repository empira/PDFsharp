<#
.SYNOPSIS
    This script waits for the download of assets to complete.

.DESCRIPTION
    This script waits for the download of assets to complete if called without the switch -InvokeDownload.
    If called with the switch -InvokeDownload, it will invoke the download script and wait for its termination.

.PARAMETER InvokeDownload
    Specifies to invoke the download script.
#>

#Requires -Version 7
#Requires -PSEdition Core


param (
    [Parameter(Mandatory = $false)] [switch]$InvokeDownload,
    [Parameter(Mandatory = $false)] [string]$Target,
    [Parameter(Mandatory = $false)] [string]$RequiredAssetsVersion
)

# Get-Location|Write-Host
# Write-Host "*$InvokeDownload*"
# Write-Host "*$Target*"
# Write-Host "*$RequiredAssetsVersion*"

$assetsFolder = "..\..\..\..\..\..\assets"
$devFolder = "..\..\..\..\..\..\dev"
$flagFile = "$download.flg"

function GetAssetsVersion() {
    try {
        # Write-Host "Entering GetAssetsVersion."
        $version = Get-Content -Path "$assetsFolder/.assets-version" -Raw -ErrorAction Stop
        $versionTrimmed = $version.Trim()
        # Write-Host "Version: *$version*"
        # Write-Host "Version: *$versionTrimmed*"
        # Write-Host "Leaving GetAssetsVersion successfully."
        return $versionTrimmed
    }
    catch {
        # Write-Host "Exception in GetAssetsVersion."
    }
    # Write-Host "Leaving GetAssetsVersion unsuccessfully."
    return "0000"
}

function CheckFlagFile([bool]$dummy) {
    $result = Test-Path $flagFile -PathType Leaf
    # Write-Host "CheckFlagFile: $result"
    return $result
}

function CreateFlagFile() {
    New-Item $flagFile -type file | Out-Null
}

function RemoveFlagFile() {
    if (Test-Path $flagFile -PathType Leaf) {
        Remove-Item $flagFile -verbose
    }
}

function UpdateRequired() {
    $current = GetAssetsVersion
    return ($RequiredAssetsVersion -gt $current)
}

function DownloadAssets() {
    # 1. Create flag file.
    # 2. Check if assets require update.
    # 3. Download assets if necessary.
    # 4. Delete flag file.

    # Write-Host "Entering DownloadAssets."
    CreateFlagFile
    if (UpdateRequired) {
        # Write-Host "Starting download-assets.ps1."
        $updateScript = ".\download-assets.ps1"
        # & "$updateScript"
        # Invoke-Expression -Command "$updateScript"
        Push-Location $devFolder
        # Get-Location|Write-Host
        & $updateScript
        Pop-Location
        # Write-Host "download-assets.ps1 has finished."
    }
    RemoveFlagFile
    # Write-Host "Leaving DownloadAssets."
}

function WaitForDownload() {
    # 1. If flag file does not exist, check if assets require update.
    # 2. If update necessary, wait a while for creation of flag file. Exit if update no longer necessary.
    # 3. If flag file exists, wait for removal of flag file.

    # Write-Output "WaitForDownload does nothing now."
    return

    # Write-Output "Entering WaitForDownload."
    $haveFlagFile = CheckFlagFile($false)
    if ($false -eq $haveFlagFile) {
        $update = UpdateRequired
        if ($false -eq $update) {
            Write-Output "No update required."
            return
        }
        # Write-Output "Update required."
        $wait = 100
        # Write-Output "Start waiting for flag file."
        $check = CheckFlagFile($false)
        while (($wait -gt 0) -and ($check -eq $false)) {
            $wait--
            # Write-Output "Waiting for flag file."
            Start-Sleep .25
            $check = CheckFlagFile($false)
        }
        # Write-Output "Finished waiting for flag file."
    }
    $check = CheckFlagFile($false)
    while ($check) {
        # Write-Output "Waiting for deletion of flag file."
        Start-Sleep .25
        $check = CheckFlagFile($false)
    }
    # Write-Output "Leaving WaitForDownload."
}

Push-Location $PSScriptRoot

if ($InvokeDownload -eq $true) {
    # Old: This should be called from the PdfSharp.GitVersion project only.
    # Old: The calling project must have just one target framework.
    # New: This should be called from the PdfSharp.BuildConfig project only.
    # New: The calling project must have just one target framework.

    # Write-Output "Master"
    DownloadAssets
}
else {
    # Write-Output "Slave"
    WaitForDownload
}

Pop-Location
