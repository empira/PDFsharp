# The PdfSharp.BuildConfig project allows us to eliminate the GitVersion build task from our projects.
# CreateBuildConfiguration.ps1 is the central workhorse of this project.
# This script collects data from git to emulate the behavior of the GitVersion build task.
# Additionally, it supports two files that can be used to overwrite the values determined from git.

# File SemVersion.json:
# This file must be in the same folder as this PowerShell script file.
# It can be used to overwrite settings when creating DLLs or NuGet packages.

# File PdfSharpBuildConfig.json:
# This file must be located in a folder above the PDFsharp repository.
# When PDFsharp is used as a submodule, this file allows overwriting settings from outside,
# allowing the project that uses PDFsharp as a submodule to create DLLs or NuGet packages
# with individual version settings.

# PDFSHARP_BUILD_VERSION are the days since 2005-01-01. #CHECK_BEFORE_RELEASE
# Command Window: ? (DateTime.Now - new DateTime(2005, 1, 1)).Days;
# C# Interactive: Console.WriteLine((DateTime.Now - new DateTime(2005, 1, 1)).Days);
# assembly-file-versioning-format: '{Major}.{Minor}.{Patch}.{env:PDFSHARP_BUILD_VERSION ?? 7342}'

# Revision: 26-03-20 StLa Works now without .git folder.
# Revision: 25-10-09 ThHo Review

# FilterBranchName: Replace characters that are not allowed in SemVer names.
function FilterBranchName([string]$name)
{
    return $name.Replace('/', '-').Replace('_', '-')
}

#CHECK_BEFORE_RELEASE
# Predefined values if we have no .git at all or git but no tag.
$NoVersionTag = "v7.0.0-preview-1"          # Default value if no git tag can be found.
$Product = "PDFsharp 7.0-Preview-1"         # Default product name.
$branchName = "release/7.0.0-preview-1"     # Default branch name if not compiled from a git repo.
$commitDate = "2026-03-23"                  # Default commit date if not compiled from a git repo.

Push-Location $PSScriptRoot

# The first directory above the PDFsharp repository.
$buildConfigFolder = "..\..\..\..\..\..\..\"

if (Test-Path -Path "..\..\..\..\..\..\.git") {
    # Get all required information directly from git.
    $branchName = git branch --show-current                           # Get name of current branch (or more general: git rev-parse --abbrev-ref HEAD).
    $commitDate = git log -1 --format="%cd" --date=format:'%Y-%m-%d'  # Get commit date in format 'YYYY-MM-DD'.
    $gitTag = git describe --abbrev=0 --tags                          # Get the last tag (required form: 'v6.2.0[-pre-release-label]').
    if ($gitTag.Length -eq 0) {
        $gitTag = $NoVersionTag
    }
    $dashPos = $gitTag.IndexOf('-')                                   # Check if pre-release label is part of the git tag.
    $gitVersionTag = $dashPos -gt 0 ? $($gitTag.Substring(0, $dashPos)) : $gitTag
    $gitLabelTag =  $dashPos -gt 0 ? $gitTag.Substring($dashPos + 1) : ""
    $buildVersion = ($(Get-Date) - $([datetime]"2005-01-01")).Days    # Days since January 1st 2005.
    $preReleaseNumber = git rev-list --count HEAD ^$gitTag            # Get number of commits since last tag.
    $majorMinorPatch = $gitVersionTag.Substring(1)                    # Git tags must start with a 'v'.
    $sha = git rev-parse HEAD                                         # Get full commit SHA.
    $shortSHA = git rev-parse --short HEAD                            # Get short commit SHA.
}
else {
    # Maybe PDFsharp is from a zip file. We must fake it.
    # $branchName is taken from above.
    # $commitDate is taken from above.
    $gitTag = $NoVersionTag
    $dashPos = $gitTag.IndexOf('-')
    $gitVersionTag = $dashPos -gt 0 ? $($gitTag.Substring(0, $dashPos)) : $gitTag
    $gitLabelTag =  $dashPos -gt 0 ? $gitTag.Substring($dashPos + 1) : ""
    $buildVersion = ($(Get-Date) - $([datetime]"2005-01-01")).Days
    $preReleaseNumber = 0
    $majorMinorPatch = $gitVersionTag.Substring(1)
    $sha = "0000000000000000000000000000000000000000"
    $shortSHA = "000000000"
}

# Read SemVersion.json.
# This file is read from the script directory.
$predefinedSemVersion = Get-Content -Path "SemVersion.json" -Raw | ConvertFrom-Json -AsHashtable
# Sample contents:
#   "UseThisInformation":  1,
#   "MajorMinorPatch": "6.9.100",
#   "SemVer": "6.9.100",
#   "InformationalVersion": "6.3.0-dev-factur-x0171",
#   "BranchName": "empira/v6.9.100",
#   "PreReleaseTag": "",
#   "PreReleaseLabel": "",
#   "PreReleaseNumber": 0,
if ($predefinedSemVersion.UseThisInformation -eq 1)
{
    $majorMinorPatch = $predefinedSemVersion.MajorMinorPatch.Length -gt 0 ? $predefinedSemVersion.MajorMinorPatch : $majorMinorPatch
    $branchName = $predefinedSemVersion.BranchName.Length -gt 0 ? $predefinedSemVersion.BranchName : $branchName
    $preReleaseLabel = $predefinedSemVersion.PreReleaseLabel.Length -gt 0 ? $predefinedSemVersion.PreReleaseLabel : $preReleaseLabel
    $preReleaseLabelOverride = $preReleaseLabel # A given tag must not be overwritten by a calculated tag.
    $preReleaseNumber = $predefinedSemVersion.PreReleaseNumber.Length -gt 0 ? $predefinedSemVersion.PreReleaseNumber : $preReleaseNumber
}
else {
    $preReleaseLabel = $gitLabelTag
}

# Read PdfSharpBuildConfig.json.
# This file is read from the first directory above the PDFsharp repository.
# Values used:
#   "SemVer": Provides "MajorMinorPatch" and "PreReleaseTag" if present.
#   "MajorMinorPatch"
#   "PreReleaseTag"
#   "BranchName"
$buildConfigPath = "$($buildConfigFolder)PdfSharpBuildConfig.json"
if (Test-Path $buildConfigPath) {
    $buildConfig = Get-Content -Path $buildConfigPath -Raw | ConvertFrom-Json -AsHashtable
}
else {
    $buildConfig = @{ }
}

# Examine SemVer, if it was specified. Split it to get MajorMinorPatch and PreReleaseTag.
if ($buildConfig.SemVer.Length -gt 0) {
    $semVersion = $buildConfig.SemVer
    $hyphenPos = $semVersion.IndexOf('-')
    # Override BranchName if SemVer is given.
    if ($hyphenPos -gt 0) {
        $majorMinorPatch = $semVersion.Substring(0, $hyphenPos)
        $preReleaseLabelOverride = $preReleaseTag = $semVersion.Substring($hyphenPos + 1) # A given tag must not be overwritten by a calculated tag.
        $branchName = "master/$($semVersion)"
    } else {
        $majorMinorPatch = $semVersion
        $preReleaseLabelOverride = $preReleaseTag = ""
        $branchName = "master/$($semVersion)"
    }
}
$majorMinorPatch = $buildConfig.MajorMinorPatch.Length -gt 0 ? $buildConfig.MajorMinorPatch : $majorMinorPatch
$Product =  $buildConfig.Product.Length -gt 0 ? $buildConfig.Product : $Product
if ($buildConfig.PreReleaseTag.Length -gt 0) {
    $preReleaseLabelOverride = $preReleaseTag = $buildConfig.PreReleaseTag # A given tag must not be overwritten by a calculated tag.
}
$branchName =  $buildConfig.BranchName.Length -gt 0 ? $buildConfig.BranchName : $branchName

# If branch name could not be determined, use "develop" as branch name.
if ($null -eq $branchName) {
    $branchName = "develop"
}

$slashPos = $branchName.LastIndexOf('/')
$shortBranchName = $slashPos -gt 0 ? $branchName.Substring($slashPos + 1) : $branchName

# Create an empty dictionary.
$semVersion = @{ }

# Get branch specific information.
$semVersion['BranchName'] = $branchName

# We allow only ASCII characters, digits, and dashes ('-') in branch names. We support accidentally added underscores.
$semVersion['EscapedBranchName'] = FilterBranchName($branchName)  # BranchName suitable for Semver.
$semVersion['ShortBranchName'] = $shortBranchName
$semVersion['CommitDate'] = $commitDate

$semVersion['BuildVersion'] = $buildVersion
$semVersion['PreReleaseNumber'] = $preReleaseNumber  # Does not depend on branch name.
$semVersion['MajorMinorPatch'] = $majorMinorPatch
# PDFsharp build number.
$semVersion['AssemblySemFileVer'] = "$majorMinorPatch.$buildVersion"  # Does not depend on branch name.
$tagSplit = [Array]$majorMinorPatch.Split(".")
$semVersion['Major'] = $tagSplit[0]
$semVersion['Minor'] = $tagSplit[1]
$semVersion['Patch'] = $tagSplit[2]

# SHA
$semVersion["SHA"] = $SHA
$semVersion["ShortSHA"] = $shortSHA
$semVersion['GitTag'] = $gitTag

# Analyze branch name to determine SemVer.
# We have/need:
#  * develop
#  * feature, bug, ...
#  * release or master
#  * anything else (user/stla/myfeatures/...)

# Cases depend on branch name.
$branch = $branchName.ToLower()

# Branch names that are to be treated like "feature".
if ($branch.StartsWith("customer/")) {
    $branch = $branch.Replace("customer/", "feature/")
}

switch -Regex ($branch) {
    '^develop$' {
        # Treat as prerelease.
        $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : "develop" + $preReleaseNumber
        $preReleaseLabel = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : "develop"
    }
    '^feature/(.*)$' {
        # Branches that begin with "feature/" will always be treated as prereleases unless the final folder begins with a version number.
        while($true) {
            $name = $($Matches[1])
            $idxSlash = $name.LastIndexOf('/')
            if ($idxSlash -ge 0) {
                $lastElement = $name.Substring($idxSlash + 1)
                # SemVer:  "^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-(0|[1-9A-Za-z-][0-9A-Za-z-]*)(\.[0-9A-Za-z-]+)*)?(\+[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$"
                $pattern = '^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)(-(0|[1-9A-Za-z-][0-9A-Za-z-]*)(\.[0-9A-Za-z-]+)*)?(\+[0-9A-Za-z-]+(\.[0-9A-Za-z-]+)*)?$'
                if ($lastElement -match $pattern) {
                    # Last element seems to be a valid Semver, so we extract version information from it.
                    $idxHyphen = $lastElement.IndexOf('-')
                    if ($idxHyphen -ge 0) {
                        # It is a prerelease.
                        $majorMinorPatch = $lastElement.Substring(0, $idxHyphen)
                        $preReleaseTag = $lastElement.Substring($idxHyphen + 1)
                        $preReleaseLabel = $preReleaseTag
                    } else {
                        # It is a final release.
                        $majorMinorPatch = $lastElement
                        $preReleaseTag = ''
                        $preReleaseLabel = ''
                    }
                    $semVersion['MajorMinorPatch'] = $majorMinorPatch
                    # PDFsharp build number.
                    $semVersion['AssemblySemFileVer'] = "$majorMinorPatch.$buildVersion"  # Does not depend on branch name.
                    $tagSplit = [Array]$majorMinorPatch.Split(".")
                    $semVersion['Major'] = $tagSplit[0]
                    $semVersion['Minor'] = $tagSplit[1]
                    $semVersion['Patch'] = $tagSplit[2]
                    break
                }
            }
            $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : "dev-$(FilterBranchName($Matches[1]))-" + $preReleaseNumber
            $preReleaseLabel = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : "dev-$(FilterBranchName($Matches[1]))"
            break
        }
    }
    '^user/(.*)$' {
        # Treat basically like "feature" for now, but do not read version from last folder.
        $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : "dev-$(FilterBranchName($Matches[1]))-" + $preReleaseNumber
        $preReleaseLabel = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : "dev-$(FilterBranchName($Matches[1]))"
    }
    # Release: "release/6.2.0-preview-3" => "preview-3"; "release/6.2.0" => ""
    '^release/(.*)$' {
        $idx = $Matches[1].IndexOf("-")
        if ($idx -lt 0) {
            $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : ""
        } else {
            $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : (FilterBranchName($Matches[1].Substring($idx + 1)))
        }
        $preReleaseLabel = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : $preReleaseTag
    }
    # Treat empira like Release for now.
    '^empira/(.*)$' {
        $idx = $Matches[1].IndexOf("-")
        if ($idx -lt 0) {
            $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : ""
        } else {
            $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : (FilterBranchName($Matches[1].Substring($idx + 1)))
        }
        $preReleaseLabel = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : $preReleaseTag
    }
    # Treat master like Release for now.
    '^master/(.*)$' {
        $idx = $Matches[1].IndexOf("-")
        if ($idx -lt 0) {
            $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : ""
        } else {
            $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : (FilterBranchName($Matches[1].Substring($idx + 1)))
        }
        $preReleaseLabel = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : $preReleaseTag
    }
    default {
        $preReleaseTag = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : ("dev-" + (FilterBranchName($branch)))
        $preReleaseLabel = -not $null -eq $preReleaseLabelOverride ? $preReleaseLabelOverride : "dev-" + (FilterBranchName($branch))
    }
}

if ($preReleaseTag -eq "") {  # Case: final release.
    $semVersion['PreReleaseTag'] = ""
    $semVersion['PreReleaseLabel'] = ""
    $semVersion['SemVer'] = "$majorMinorPatch"
    $semVersion['AssemblySemFileVer'] = "$majorMinorPatch.$buildVersion"
    $semVersion['InformationalVersion'] = "$majorMinorPatch"
} else {  # Case: pre-release.
    $semVersion['PreReleaseTag'] = $preReleaseTag
    $semVersion['PreReleaseLabel'] = $preReleaseLabel
    $semVersion['SemVer'] = "$majorMinorPatch-$preReleaseTag"
    $semVersion['AssemblySemFileVer'] = "$majorMinorPatch.$buildVersion"
    $semVersion['InformationalVersion'] = "$majorMinorPatch-$preReleaseLabel-$preReleaseNumber"
}

# Define template for SemVersion.props.
$templateSemVerProps = @"
<Project>
  <!-- Properties generated by PdfSharp.BuildConfig -->
  <PropertyGroup>

    <Version>{Version}</Version>
    <FileVersion>{FileVersion}</FileVersion>
    <InformationalVersion>{InformationalVersion}</InformationalVersion>
    <Major>{Major}</Major>
    <Minor>{Minor}</Minor>
    <Patch>{Patch}</Patch>
    <PreReleaseLabel>{PreReleaseLabel}</PreReleaseLabel>
    <SemVer>{SemVer}</SemVer>
    <BranchName>{BranchName}</BranchName>
    <CommitDate>{CommitDate}</CommitDate>
    <Sha>{Sha}</Sha>
    <ShortSha>{ShortSha}</ShortSha>

  </PropertyGroup>
</Project>
"@

$outputSemVerProps = $templateSemVerProps `
    -replace "{Version}", $semVersion['MajorMinorPatch'] `
    -replace "{FileVersion}", $semVersion['AssemblySemFileVer'] `
    -replace "{InformationalVersion}", $semVersion['InformationalVersion'] `
    -replace "{Major}", $semVersion['Major'] `
    -replace "{Minor}", $semVersion['Minor'] `
    -replace "{Patch}", $semVersion['Patch'] `
    -replace "{PreReleaseLabel}", $semVersion['PreReleaseLabel'] `
    -replace "{SemVer}", $semVersion['SemVer'] `
    -replace "{BranchName}", $semVersion['BranchName'] `
    -replace "{CommitDate}", $semVersion['CommitDate'] `
    -replace "{Sha}", $semVersion['Sha'] `
    -replace "{ShortSha}", $semVersion['ShortSha']

# Read old file.
$oldOutputSemVerProps = "<empty>"
try {
    $oldOutputSemVerProps = Get-Content -Path "../../../../../SemVersion.props" -Raw -ErrorAction Stop
}
catch {
    $oldOutputSemVerProps = "<reading failed>"
}

# Write SemVersion.props in {solution-root}/src if contents have changed.
if ($oldOutputSemVerProps.Length -eq 0 -or -not $oldOutputSemVerProps.Contains($outputSemVerProps)) {
  $outputSemVerProps | Out-File -FilePath "../../../../../SemVersion.props"
}
else {
  # Write-Output "Skipped writing SemVersion.props because contents did not change."
}

$templateSemVerInfo = @"
// PDFsharp - A .NET library for processing PDF
// See the LICENSE file in the solution root for more information.

//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by a build step of project PdfSharp.BuildConfig.
//
//   Changes to this file may cause incorrect behavior and will be lost if
//   the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PdfSharp.Internal
{
    public static partial class SemVersionInformation
    {
        const string SemMajor = "{Major}";
        const string SemMinor = "{Minor}";
        const string SemPatch = "{Patch}";
        const string SemVersion = "{SemVersion}";
        const string SemFileVersion = "{FileVersion}";
        const string SemPreReleaseLabel = "{PreReleaseLabel}";
        const string SemInformationalVersion = "{InformationalVersion}";
        const string SemBranchName = "{BranchName}";
        const string SemCommitDate = "{CommitDate}";
        const string SemSha = "{Sha}";
        const string SemShortSha = "{ShortSha}";
    }
}
"@

# Replace placeholders.
$outputSemVerInfo = $templateSemVerInfo `
    -replace "{Major}", $semVersion['Major'] `
    -replace "{Minor}", $semVersion['Minor'] `
    -replace "{Patch}", $semVersion['Patch'] `
    -replace "{SemVersion}", $semVersion['SemVer'] `
    -replace "{FileVersion}", $semVersion['AssemblySemFileVer'] `
    -replace "{PreReleaseLabel}", $semVersion['PreReleaseLabel'] `
    -replace "{InformationalVersion}", $semVersion['InformationalVersion'] `
    -replace "{BranchName}", $semVersion['BranchName'] `
    -replace "{CommitDate}", $semVersion['CommitDate'] `
    -replace "{Sha}", $semVersion['Sha'] `
    -replace "{ShortSha}", $semVersion['ShortSha']

# Read old file.
$oldOutputSemVerInfo = "<empty>"
try {
    $oldOutputSemVerInfo = Get-Content -Path "../PdfSharp.Shared/internal/SemVersionInformation-generated.cs" -Raw -ErrorAction Stop
}
catch {
    $oldOutputSemVerInfo = "<reading failed>"
}
# Write SemVersionInformation-generated.cs if contents have changed.
if ($oldOutputSemVerInfo -eq 0 -or -not $oldOutputSemVerInfo.Contains($outputSemVerInfo)) {
    $outputSemVerInfo | Out-File -FilePath "../PdfSharp.Shared/internal/SemVersionInformation-generated.cs"
}
else {
    # Write-Output "Skipped writing SemVersionInformation-generated.cs because contents did not change."
}

###################

# Define template for BuildConfig.props.
$templateBuildConfigProps = @"
<Project>
  <!-- Properties generated by PdfSharp.BuildConfig -->
  <PropertyGroup>
    <PDFsharp_PackageName>{PDFsharp_PackageName}</PDFsharp_PackageName>
    <PDFsharpMigraDoc_PackageName>{PDFsharpMigraDoc_PackageName}</PDFsharpMigraDoc_PackageName>
    <Product>{Product}</Product>
  </PropertyGroup>
</Project>
"@

if ($buildConfig.Length -ne 0) {
  $outputBuildConfigProps = $templateBuildConfigProps `
      -replace "{PDFsharp_PackageName}", $buildConfig['PDFsharp_PackageName'] `
      -replace "{PDFsharpMigraDoc_PackageName}", $buildConfig['PDFsharpMigraDoc_PackageName'] `
      -replace "{Product}", $buildConfig['Product'] `

  # Read old file.
  $oldOutputBuildConfigProps = "<empty>"
  try {
      $oldOutputBuildConfigProps = Get-Content -Path "../../../../../BuildConfig.props" -Raw -ErrorAction Stop
  }
  catch {
      $oldOutputBuildConfigProps = "<reading failed>"
  }

  # Write PdfSharpBuildConfig.props if contents have changed.
  if ($oldOutputBuildConfigProps.Length -eq 0 -or -not $oldOutputBuildConfigProps.Contains($outputBuildConfigProps)) {
    $outputBuildConfigProps | Out-File -FilePath "../../../../../PdfSharpBuildConfig.props"
  }
  else {
      # Write-Output "Skipped writing PdfSharpBuildConfig.props because contents did not change."
  }
}

Pop-Location

# Contents of SemVersion.props file generated by this script.
# BuildConfig: Can be set in PdfSharpBuildConfig.json located outside the repository so the project using PDFsharp as a submodule can control the names of the NuGet packages.
#
#                                                       # Build    Comments
# {                                                     # Config
#   "Version": "6.3.0",                                 #   ✔      "MajorMinorPatch" in BuildConfig.
#   "FileVersion": "6.3.0.7443",                        #           The 4th number is essential for installer software. It counts the days since January 1st 2005.
#   "InformationalVersion": "6.3.0-develop0001",        #
#   "Major": 6,                                         #
#   "Minor": 3,                                         #
#   "Patch": 0,                                         #
#   "PreReleaseLabel": "develop",                       #   ✔      Set from "PreReleaseTag" in BuildConfig.
#   "SemVer": "6.3.0-develop.1",                        #   ✔      Also used for NuGet.
#   "BranchName": "develop",                            #   ✔
#   "CommitDate": "2025-06-07",                         #
#   "Sha": "a8e9a5dd283153ef472b755ee392651d8c6e9c99",  #           TODO: Write in PDF file.
#   "ShortSha": "a8e9a5d"                               #           TODO: Write in PDF file.
# }
