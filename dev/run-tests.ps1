#Requires -Version 7
#Requires -PSEdition Core

# Gets DllInfo objects for all DLLs of the solution to run dotnet test in WSL for.
function GetWslTestDllInfos($solution) {
    $dllInfos = GetDllInfos $solution

    $testDllInfos = $dllInfos | Where-Object { $_.IsTestDll -eq $true }
    $testDllsOutput = $testDllInfos | Select-Object -Property DllFileName, DllFolder

    Write-Host
    Write-Host "DLLs to test found in `"$solution`""
    Write-Host "(only used to determine DLLs to test in WSL; in Windows dotnet test is run for the solution):"
    Write-Host ----------------------------------------
    Write-Host ($testDllsOutput | Format-Table | Out-String)

    $wslTestDllInfos = $testDllInfos | Where-Object {$_.DllFileName.EndsWith("-gdi.dll", "OrdinalIgnoreCase") -eq $false -and $_.DllFileName.EndsWith("-wpf.dll", "OrdinalIgnoreCase") -eq $false}
    $wslTestDllsOutput = $wslTestDllInfos | Select-Object -Property DllFileName, DllFolder

    Write-Host "DLLs to test in WSL found in `"$solution`":"
    Write-Host ----------------------------------------
    Write-Host ($wslTestDllsOutput | Format-Table | Out-String)

    return $wslTestDllInfos
}

# Gets DllInfo objects for all DLLs of the solution.
function GetDllInfos($solution) {
    $projects = GetSolutionProjects $Solution
    $dllInfos = $projects | ForEach-Object {GetDllInfo $_}

    return $dllInfos
}

# Creates a DllInfo object for a project.
function GetDllInfo($project) {
    $projectFolder = Split-Path -Path $project | Resolve-Path -Relative
    $projectName = Split-Path -LeafBase $project

    $debugFolder = Join-Path -Path $projectFolder "bin\debug"
    $dllFile = Get-ChildItem -Path $debugFolder -Filter "$projectName.dll" -Recurse -ErrorAction SilentlyContinue -Force | Select-Object  -first 1
    
    if ($dllFile -eq $null) {
        Write-Error "Could not finde file `"$debugFolder\**\$projectName.dll`". Maybe Debug Build has not been built."
    }

    $dllFolder = $dllFile.Directory.FullName | Resolve-Path -Relative
    $dllFileName = $dllFile.Name

    $isTestDll = ContainsTestPlatformDll $dllFolder

    $info = New-Object -TypeName psobject
    $info | Add-Member -MemberType NoteProperty -Name ProjectName -Value $projectName
    $info | Add-Member -MemberType NoteProperty -Name ProjectFolder -Value $projectFolder
    $info | Add-Member -MemberType NoteProperty -Name DllFileName -Value $dllFileName
    $info | Add-Member -MemberType NoteProperty -Name DllFolder -Value $dllFolder
    $info | Add-Member -MemberType NoteProperty -Name IsTestDll -Value $isTestDll

    return $info
}

# Examines if $path contains "Microsoft.TestPlatform.CommunicationUtilities.dll".
# If so, we can assume that the project DLL in $path contains unit tests to be run.
# This information is useful because starting non test projects in dotnet test may return errors.
function ContainsTestPlatformDll($path) {
    $testDlls = Get-ChildItem -Path $path -Filter "Microsoft.TestPlatform.CommunicationUtilities.dll" -Recurse -ErrorAction SilentlyContinue -Force
    $result = ($testDlls | measure | Select-Object -ExpandProperty Count) -gt 0

    return $result
}

# Gets the projects of a solution.
function GetSolutionProjects($solutionFileName) {
    $entries = dotnet sln $solutionFileName list
    $projects = $entries | Where-Object {$_.EndsWith(".csproj") -eq $true}
    return $projects
}

# Gets the only solution in the current folder.
function GetSolutionFileName() {
    $solutionFile = Get-ChildItem -Filter "*.sln" -Recurse -ErrorAction SilentlyContinue -Force | Select-Object -first 1
    $solutionFileName = $solutionFile.Name
    return $solutionFileName
}

function AddTestResultsForEnvironment([ref]$testResults, [ref]$environmentNames, $timeStart, $testResultsFilename, $environmentName) {
    AddEnvironment $environmentNames $environmentName

    $testResultFiles = GetTestResultFiles $testResultsFilename $environmentName $timeStart

    foreach ($testResultFile in $testResultFiles) {
        AddTestResultsFromFile $testResults $environmentNames $testResultFile $environmentName
    }
}

function AddTestResultsFromFile([ref]$testResults, [ref]$environmentNames, $path, $environmentName) {
    $environmentResults = GetTestResults $path $environmentName
    $testResults.Value += $environmentResults
    AddEnvironment $environmentNames $environmentName
}

function AddEnvironment([ref]$environmentNames, $environmentName) {
    if ($environmentNames.Value -notcontains $environmentName) {
        $environmentNames.Value += $environmentName
    }
}

function GetTestResultFiles($testResultsFilename, $environmentName, $timeStart) {
    $testResultFiles = Get-ChildItem -Filter $testResultsFilename -Recurse -ErrorAction SilentlyContinue -Force | Where-Object LastWriteTime -gt $timeStart

    Write-Host
    Write-Host "$environmentName test files:"
    if (($testResultFiles | measure).Count -eq 0) {
        Write-Host "No test results found."
        return $testResultFiles
    }

    $testResultFiles = ($testResultFiles).FullName | Resolve-Path -Relative

    foreach ($testResultFile in $testResultFiles) {
        Write-Host ($testResultFile)
    }

    return $testResultFiles
}

function GetTestResults($path, $environmentName) {
    $fileContent = ReadTestResultXml $path

    $nameSpace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"
    $nameSpaceManager = new-object Xml.XmlNamespaceManager $fileContent.NameTable
    $nameSpaceManager.AddNamespace("ns", $nameSpace)

    $testResults = @()
    foreach($unitTestResult in $fileContent.SelectNodes('//ns:UnitTestResult', $nameSpaceManager)) {
        $testResult = New-Object -TypeName PsObject

        $testResult | Add-Member -MemberType NoteProperty -Name ExecutionId -Value $unitTestResult.executionId
        $testResult | Add-Member -MemberType NoteProperty -Name TestName -Value $unitTestResult.testName
        $testResult | Add-Member -MemberType NoteProperty -Name Outcome -Value $unitTestResult.outcome

        # Not used:
        #$testResult | Add-Member -MemberType NoteProperty -Name Duration -Value $unitTestResult.duration
        #$testResult | Add-Member -MemberType NoteProperty -Name Message -Value $unitTestResult.Output.ErrorInfo.Message
        #$testResult | Add-Member -MemberType NoteProperty -Name StackTrace -Value $unitTestResult.Output.ErrorInfo.StackTrace

        $testResult | Add-Member -MemberType NoteProperty -Name Environment -Value $environmentName

        $testResults += $testResult
    }

    foreach($testResult in $testResults){
        $testDefinition = $fileContent.SelectNodes("//ns:UnitTest/ns:Execution[@id='" + $testResult.ExecutionId + "']", $nameSpaceManager)

        $codeBase = GetWindowsPath $testDefinition.NextSibling.codeBase | Resolve-Path -Relative

        $testResult | Add-Member -MemberType NoteProperty -Name CodeBase -Value $codeBase
    }

    return $testResults
}

function GetWindowsPath($path) {
    if ($path -match "^/mnt/(?<drive>\w)/") {
        #$path = wsl -e wslpath -w $path
        # Do manual folder conversion as calling wslpath is very very slow.
        $path = -join($Matches.drive.ToUpper(), ":\", $path.Substring(7).Replace('/', '\'))
    }
    return $path
}

function ReadTestResultXml($path) {
    Write-Debug "Reading $path and parse as XML"
    [xml]$fileContent = Get-Content -Path $path
    return $fileContent
}

function GroupTestResults($testResults, $environmentNames) {
    $codeBaseGroups = $testResults | Group-Object CodeBase | Sort-Object Name

    $groupedResults = @()
    foreach ($codeBaseGroup in $codeBaseGroups) {
        $testNameGroups = $codeBaseGroup.Group | Group-Object TestName | Sort-Object Name
        foreach ($testNameGroup in $testNameGroups) {
            $groupedResult = New-Object -TypeName PsObject

            $groupedResult | Add-Member -MemberType NoteProperty -Name TestName -Value $testNameGroup.Name
            $groupedResult | Add-Member -MemberType NoteProperty -Name CodeBase -Value $codeBaseGroup.Name
            foreach ($environmentName in $environmentNames) {
                $testResult = $testNameGroup.Group | Where-Object Environment -EQ $environmentName | Select-Object -First 1
                $outcome = $testResult.Outcome
                if ($null -eq $outcome) {
                    $outcome = "Not found"
                }
                $groupedResult | Add-Member -MemberType NoteProperty -Name $environmentName -Value $outcome
            }

            $groupedResults += $GroupedResult
        }
    }

    return $groupedResults
}

function AddTestSummary($groupedResults, $environmentNames) {
    $groupedResultsWithSummary = @()
    foreach ($groupedResult in $groupedResults) {
        $groupedResultsWithSummary += $groupedResult
    }

    $emptyLine = New-Object -TypeName PsObject
    $groupedResultsWithSummary += $emptyLine

    $emptyLine = New-Object -TypeName PsObject
    $groupedResultsWithSummary += $emptyLine

    $emptyLine = New-Object -TypeName PsObject
    $emptyLine | Add-Member -MemberType NoteProperty -Name TestName -Value "========================================"
    foreach ($environmentName in $environmentNames) {
        $emptyLine | Add-Member -MemberType NoteProperty -Name $environmentName -Value "=============="
    }
    $groupedResultsWithSummary += $emptyLine

    $passedLine = New-Object -TypeName PsObject
    $passedLine | Add-Member -MemberType NoteProperty -Name TestName -Value "    Total:"
    foreach ($environmentName in $environmentNames) {
        $count = ($groupedResults | Where-Object $environmentName -EQ "Passed" | Measure-Object).Count
        $passedLine | Add-Member -MemberType NoteProperty -Name $environmentName -Value "Passed:    $count"
    }
    $groupedResultsWithSummary += $passedLine


    $failedLine = New-Object -TypeName PsObject
    foreach ($environmentName in $environmentNames) {
        $count = ($groupedResults | Where-Object $environmentName -EQ "Failed" | Measure-Object).Count
        $failedLine | Add-Member -MemberType NoteProperty -Name $environmentName -Value "Failed:    $count"
    }
    $groupedResultsWithSummary += $failedLine


    $notFoundLine = New-Object -TypeName PsObject
    foreach ($environmentName in $environmentNames) {
        $count = ($groupedResults | Where-Object $environmentName -EQ "Not found" | Measure-Object).Count
        $notFoundLine | Add-Member -MemberType NoteProperty -Name $environmentName -Value "Not found: $count"
    }
    $groupedResultsWithSummary += $notFoundLine

    return $groupedResultsWithSummary
}



function ColorizedCellFormatExpressionResult($property) {
    $isLine = $property -match "[=]"

    $split = $property.Split()
    $count = $null;
    if ($split.Length -ge 2) {
        $split2 = $split[$split.Length - 1]
        $parsedCount = 0;
        if (([System.Int32]::TryParse($split2, [ref] $parsedCount))) {
            $count = $parsedCount
        }
    }

    # Color codes see https://duffney.io/usingansiescapesequencespowershell/#basic-foreground-background-colors
    $color = "33" # Orange

    if ($isLine -or $count -eq 0) { # Don't change color, if a count of 0 is given.
        $color = "37" # White
    }
    elseif ($property.ToLower().Contains("failed")) {
        $color = "31" # Red
    }
    elseif ($property.ToLower().Contains("passed")) {
        $color = "32" # Green
    }

    $e = [char]27
    "$e[${color}m$($property)${e}[0m"
}

function SaveForegroundColor() {
    $script:foregroundColorBackup = $host.UI.RawUI.ForegroundColor
}

function RestoreForegroundColor() {
    $host.UI.RawUI.ForegroundColor = $script:foregroundColorBackup
}


# ***** Main *****

$TestResultsFilenameWindows = "test-windows.trx"
$TestResultsFilenameWSL = "test-wsl.trx"

$TimeStart = Get-Date
#$TimeStart = Get-Date -Year 1900 # Hack for loading all found test result files and not only the ones created in this run.

# TODO Add try/finally for each Push-Location.
Push-Location $PSScriptRoot
Push-Location ..

SaveForegroundColor

Write-Host
$Solution = GetSolutionFileName
Write-Host "Started run-tests for solution `"$Solution`"."

$wslTestDllInfos = GetWslTestDllInfos $Solution

Write-Host Running tests under local Windows
Write-Host ==================================================
Write-Host
Write-Host dotnet test $Solution...
Write-Host ----------------------------------------
Write-Host
dotnet test $Solution --no-build -l "trx;LogFileName=.\$TestResultsFilenameWindows"
RestoreForegroundColor # The dotnet call may change the foreground color, e. g. when displaying test result exceptions.


Write-Host
Write-Host Running tests under WSL
Write-Host ==================================================
foreach ($wslTestDllInfo in $wslTestDllInfos) {
    $wslTestDll = Join-Path $wslTestDllInfo.DllFolder $wslTestDllInfo.DllFileName
    $wslTestDllAbsolute = Resolve-Path $wslTestDll
    $wslTestDllLinux = wsl wslpath -u $wslTestDllAbsolute

    Write-Host
    Write-Host WSL: dotnet test $wslTestDllInfo.DllFileName...
    Write-Host ----------------------------------------
    Write-Host

    # Change current location to get the log file stored in the default "TestResults" folder inside the project folder.
    Push-Location $wslTestDllInfo.ProjectFolder

    wsl -e dotnet test $wslTestDllLinux -l "trx;LogFileName=./$TestResultsFilenameWSL"
    RestoreForegroundColor # The dotnet call may change the foreground color, e. g. when displaying test result exceptions.

    Pop-Location
}


# Load and prepare the created TRX test result files.
Write-Host
Write-Host Preparing test results
Write-Host =========================
Write-Host

$EnvironmentNameWindows = "Windows"
$EnvironmentNameWSL = "WSL"

$Results = @()
$EnvironmentNames = @()
AddTestResultsForEnvironment ([ref]$Results) ([ref]$EnvironmentNames) $TimeStart $TestResultsFilenameWindows $EnvironmentNameWindows
AddTestResultsForEnvironment ([ref]$Results) ([ref]$EnvironmentNames) $TimeStart $TestResultsFilenameWSL $EnvironmentNameWSL

$GroupedResults = GroupTestResults $Results $EnvironmentNames

$GroupResultsWithSummary = AddTestSummary $GroupedResults $EnvironmentNames

Write-Host
Write-Host "TestResults" -ForegroundColor Green
Write-Host "==================================================" -ForegroundColor Green

# Cannot generate $formats automatically, as Expression is executed later.
# In a loop generating the formats, all Expressions would get only the last assigned value of the loop variable.
$formats = @(
    @{
        Label = "Test"
        Expression = { $_.TestName }
        Width = 86
        },
    @{
        Label = $EnvironmentNameWindows
        Expression = { ColorizedCellFormatExpressionResult($_.Windows) }
        Width = 17
    },
    @{
        Label = $EnvironmentNameWSL
        Expression = { ColorizedCellFormatExpressionResult($_.WSL) }
        Width = 17
    }
)

$GroupResultsWithSummary | Format-Table $formats -Wrap -GroupBy CodeBase

Pop-Location
Pop-Location
