<#
.SYNOPSIS
    This script runs 'dotnet test' in all available environments for the solution found in the script’s root parent folder.

.DESCRIPTION
    The script builds the solution located in the script’s root parent folder and runs 'dotnet test' for all libraries to test, that are found via its projects.
    These tests are run in the following environment, as far as available: Windows with NET8 or NET10, Windows with NET462 and Linux/WSL (NET8 or NET10).
    For each environment, libraries not to be run (like WPF in Linux or Linux-targeting DLLs in Windows) are excluded from testing.
    The test results are displayed in tables per library / code base comparing the test results in the different environments.

.PARAMETER Config
    Specifies the configuration to build and test the solution ("Debug" or "Release"). "Debug" is the default.

.PARAMETER Net10
    Specifies whether NET10 shall be tested instead of NET8. $False is the default.

.PARAMETER SkipBuild
    Specifies whether the build of the solution shall be skipped. $False is the default.

.PARAMETER RunAllTests
    Specifies whether to run even the slow tests, whose execution can be managed via PDFsharpTests environment variable. $False is the default.

.NOTES
    Possible test results are:
    --------------------------

    Passed:           The test ran successfully.

    Failed:           The test has failed.

    Skipped:          The test was marked to be skipped (or another circumstance resulting in 'NotExecuted' in the trx file occurred).

    Not implemented:  The test result was only found for other environments. Maybe the project is not targeting or the test is not implemented for this environment.

    Not applicable:   The test library was not expected to be executed, as it is not intended to be run in this environment or as this environment is not available.

    No trx file:      The test library was expected to be executed, but no trx file was found. Maybe an error occurred in this script or in the 'dotnet test' call.

    Calling the script:
    -------------------

    If started in Windows, tests are executed in Windows and WSL:

        > .\dev\run-tests.ps1

    If started from Windows in WSL, tests are executed in WSL only:

        > wsl -e pwsh -c .\dev\run-tests.ps1

    If started in Linux / WSL, tests are executed in Linux / WSL only:

        > pwsh -c ./dev/run-tests.ps1

    Changing the script:
    --------------------

    The master version of this script is maintained in the PDFsharp repository. Copies may be distributed to other repositories.
    If changes to the script are needed, implement them in the PDFsharp repository and update all copies of the script in other repositories.
#>

<#
BUG: Allow to run all tests, including GBE.
#>

#Requires -Version 7
#Requires -PSEdition Core


param (
    [Parameter(Mandatory = $false)] [string]$Config = 'Debug',
    [Parameter(Mandatory = $false)] [bool]$Net10 = $false,
    [Parameter(Mandatory = $false)] [bool]$SkipBuild = $false,
    [Parameter(Mandatory = $false)] [bool]$RunAllTests = $false
)

$script:SystemNameWindows = "Windows"
$script:SystemNameLinux = "Linux"
$script:SystemNameWsl = "WSL"
$script:NetName462 = "net462"
$script:NetName10 = "net10"
$script:NetName8 = "net8"


$script:Solution
$script:TimeStart
$script:ConsoleWidth

# Current system information.
$script:SystemNameCurrentLinux
$script:SystemNameHost
$script:RunOnWindowsHost
$script:RunOnLinuxHost
$script:RunOnHostedWsl
$script:EnvironmentForHostedWsl

# Information about DLLs to test in Windows/Linux.
$script:TestDllInfosWindows
$script:TestDllInfosLinux


function InitializeScript()
{
    SaveForegroundColor

    $script:Solution = GetSolutionFileName

    $script:Config = (Get-Culture).TextInfo.ToTitleCase($script:Config)

    if ($script:Config -ne "Debug" -and $script:Config -ne "Release")
    {
        Write-Error "Configuration ($script:Config) is currently not supported. Please pass `"Debug`" or `"Release`" as parameter."
        exit
    }

    Write-Output ""
    Write-Output "Started run-tests for solution `"$script:Solution`"."
    Write-Output ""

    $script:TimeStart = Get-Date
    $script:ConsoleWidth = $Host.UI.RawUI.WindowSize.Width
    if ($script:ConsoleWidth -lt 80)
    {
        Write-Warning "Low console width recognized ($script:ConsoleWidth)!"
        Write-Warning "A width of at least 80 characters is recommended to ensure a good presentation. For lower widths result columns may be omitted."
    }

    # Pre-set current system information.
    $script:SystemNameCurrentLinux = "none"
    $script:SystemNameHost = "unknown"
    $script:RunOnWindowsHost = $false
    $script:RunOnLinuxHost = $false
    $script:RunOnHostedWsl = $false

    # Set current system information.
    if ($IsWindows)
    {
        $script:RunOnWindowsHost = $true
        $script:SystemNameHost = $script:SystemNameWindows

        # Check if WSL is installed.
        try
        {
            wslconfig /l | Out-Null

            $script:RunOnHostedWsl = $true
            $script:SystemNameCurrentLinux = $script:SystemNameWsl
        }
        catch
        {
            #$script:RunOnHostedWsl remains false.
        }
    }
    elseif ($IsLinux)
    {
        $script:RunOnLinuxHost = $true

        # Check if Linux is WSL.
        if (Test-Path "/proc/sys/fs/binfmt_misc/WSLInterop" -PathType Leaf)
        {
            $script:SystemNameCurrentLinux = $script:SystemNameWsl
        }
        else
        {
            $script:SystemNameCurrentLinux = $script:SystemNameLinux
        }
        $script:SystemNameHost = $script:SystemNameCurrentLinux
    }

    # Output current system information.
    Write-Output ""
    Write-Output "Started script in $script:SystemNameHost."
    if ($script:RunOnHostedWsl)
    {
        Write-Output "Tests will be run on $script:SystemNameHost host and hosted $script:SystemNameCurrentLinux."
    }
    else
    {
        Write-Output "Tests will be run on $script:SystemNameHost host only."
    }
    Write-Output ""

    if ($script:Net10)
    {
        Write-Output "NET10 Tests will be run instead of NET8."
        Write-Output ""
    }

    if ($script:SkipBuild)
    {
        Write-Output "Building solution in $script:Config build will be skipped."
        Write-Output ""
    }

    if ($script:RunAllTests)
    {
        $env:PDFsharpTests = "runalltests"
        $script:EnvironmentForHostedWsl = "PDFsharpTests=runalltests"
        Write-Output "Running all tests of solution."
    }
    else
    {
        $env:PDFsharpTests = $null
        $script:EnvironmentForHostedWsl = "PDFsharpTests=$null"
        Write-Output "Skipping slow tests of solution."
    }
    Write-Output ""

    CheckNetRuntimes
}

# Checks the net runtimes for all available environments.
function CheckNetRuntimes() {
    # Check net runtime for local machine.
    CheckNetRuntime $false

    # Check net runtime for hosted WSL, if needed.
    if ($script:RunOnHostedWsl) {
        CheckNetRuntime $true
    }
}

# Checks the net runtime for the local machine or WSL.
function CheckNetRuntime($isWsl)
{
    if ($isWsl) {
        $runtimes = (wsl -e dotnet --list-runtimes | Out-String) -split "`n"
        $wslOrLocal = "WSL"
    }
    else {
        $runtimes = (dotnet --list-runtimes | Out-String) -split "`n"
        $wslOrLocal = "the local machine"
    }

    if ($script:Net10) {
        $netMajorVersion = 10;
    }
    else {
        $netMajorVersion = 8;
    }

    $hasRequiredVersion = ($runtimes | Where-Object { $_.StartsWith("Microsoft.NETCore.App $netMajorVersion.") } | Measure-Object | Select-Object -ExpandProperty Count) -gt 0

    if ($hasRequiredVersion -eq $false)
    {
        Write-Error "The script is configured to run tests for net$netMajorVersion, but the net$netMajorVersion runtime is not installed on $wslOrLocal."
        exit
    }
}

# Gets the first solution in the current folder. There should be only one.
function GetSolutionFileName() {
    $solutionFile = Get-ChildItem -Filter "*.sln" -Recurse -ErrorAction SilentlyContinue -Force | Select-Object -first 1
    $solutionFileName = $solutionFile.Name
    return $solutionFileName
}


# Builds the Solution.
function BuildSolution()
{
    if ($script:SkipBuild)
    {
        return
    }

    Write-Output ""
    Write-Output "Building solution in $script:Config build"
    Write-Output ==================================================
    Write-Output ""

    dotnet build $script:Solution -c $script:Config
    RestoreForegroundColor # The dotnet call may change the foreground color, e. g. when displaying test result exceptions.
}


# Loads DllInfo objects for all DLLs of the solution to run dotnet test for and saves the Windows and Linux specific lists.
function LoadTestDllInfos()
{
    Write-Output ""
    Write-Output "Analyzing Solution to find test DLLs"
    Write-Output ==================================================
    Write-Output ""

    $dllInfos = GetDllInfos

    $testDllInfos = $dllInfos | Where-Object { $_.IsTestDll }

    # If Net10 parameter is true, remove net8 DLLs.
    if ($script:Net10)
    {
        $testDllInfos = $testDllInfos | Where-Object `
        {
            $_.TargetFramework.Contains("net8") -eq $false
            $_.TargetFramework.Contains("net9") -eq $false
        }
    }
    # If Net10 parameter is false, remove net10 DLLs.
    else
    {
        $testDllInfos = $testDllInfos | Where-Object `
        {
            $_.TargetFramework.Contains("net10") -eq $false
            $_.TargetFramework.Contains("net9") -eq $false
        }
    }

    # Test-HACK: Only include explicit projects.
    #$testDllInfos = $testDllInfos | Where-Object {$_.DllFileName.EndsWith("PdfSharp.Tests.dll", "OrdinalIgnoreCase") -or $_.DllFileName.EndsWith("Shared.Tests.dll", "OrdinalIgnoreCase")}

    # Set $script:TestDllInfosWindows list if running on Windows host.
    if ($script:RunOnWindowsHost)
    {
        # Exclude Linux target frameworks for Windows.
        $script:TestDllInfosWindows = $testDllInfos | Where-Object `
        {
            $_.TargetFramework.Contains("linux") -eq $false
        } | ForEach-Object { $_.PSObject.Copy() }

        AddEnvironmentName $script:TestDllInfosWindows $script:SystemNameWindows

        OutputTestDlls $script:TestDllInfosWindows $script:SystemNameWindows
    }

    # Set $script:TestDllInfosLinux list if running on Linux host or if tests will run in hosted WSL.
    if ($script:RunOnLinuxHost -or $script:RunOnHostedWsl)
    {
        # Exclude WPF and GDI projects and net462 and Windows target frameworks for Linux.
        $script:TestDllInfosLinux = $testDllInfos | Where-Object `
        {
            $_.DllFileName.EndsWith("-gdi.dll", "OrdinalIgnoreCase") -eq $false -and `
            $_.DllFileName.EndsWith("-wpf.dll", "OrdinalIgnoreCase") -eq $false -and `
            $_.TargetFramework.Contains("net462") -eq $false -and `
            $_.TargetFramework.Contains("windows") -eq $false
        } | ForEach-Object { $_.PSObject.Copy() }

        AddEnvironmentName $script:TestDllInfosLinux $script:SystemNameCurrentLinux

        OutputTestDlls $script:TestDllInfosLinux $script:SystemNameCurrentLinux
    }
}

function AddEnvironmentName($testDllInfos, $systemName)
{
    foreach($testDllInfo in $testDllInfos)
    {
        # Get the environment name and the trx filename by $systemName and the target framework of the $testDllInfo.
        $environmentName = GetEnvironmentName $systemName $testDllInfo.TargetFramework

        $testDllInfo | Add-Member -MemberType NoteProperty -Name EnvironmentName -Value $environmentName
    }
}

function OutputTestDlls($testDllInfos, $systemName)
{
    $testDllsOutput = $testDllInfos | Select-Object -Property DllFileName, EnvironmentName, DllFolder

    Write-Output "DLLs to test in $systemName found in `"$script:Solution`":"
    Write-Output ----------------------------------------
    Write-Output ($testDllsOutput | Format-Table | Out-String)
}

# Gets DllInfo objects for all DLLs of the solution.
function GetDllInfos() {
    $projects = GetSolutionProjects $script:Solution

    $dllInfos = New-Object System.Collections.Generic.List[System.Object]
    $exeGenericCodeBases = New-Object System.Collections.Generic.List[System.Object]

    # Add dllInfo for each target framework of each project to $dllInfos.
    foreach($project in $projects)
    {
        $projectFolder = Split-Path -Path $project | Resolve-Path -Relative
        $projectName = Split-Path -LeafBase $project

        $binFolder = Join-Path -Path $projectFolder "bin/$script:Config"

        $binFolderExists = Test-Path $binFolder
        if ($binFolderExists -eq $false)
        {
            Write-Error "Missing bin folder `"$binFolder`". Maybe an error occurred while building the solution."
        }

        $targetFrameworkPaths = Get-ChildItem -Path $binFolder -Directory
        if (($targetFrameworkPaths | Measure-Object | Select-Object -ExpandProperty Count) -eq 0)
        {
            Write-Error "No target framework folders found in bin folder `"$binFolder`". Maybe an error occurred while building the solution."
        }

        foreach($targetFrameworkPath in $targetFrameworkPaths)
        {
            $targetFrameworkFolder = Split-Path -Leaf $targetFrameworkPath
            $targetFramework = $targetFrameworkFolder

            # Get DLL or, if not found, exe file belonging to the project.
            $dllFile = Get-ChildItem -Path $targetFrameworkPath -Filter "$projectName.dll" -Recurse -ErrorAction SilentlyContinue -Force
            if ($null -ne $dllFile)
            {
                $isExeFile = $false
            }
            else
            {
                $dllFile = Get-ChildItem -Path $targetFrameworkPath -Filter "$projectName.exe" -Recurse -ErrorAction SilentlyContinue -Force
                if ($null -ne $dllFile)
                {
                    $isExeFile = $true
                }
            }

            if ($null -eq $dllFile)
            {
                Write-Error "Could not find file `"$targetFrameworkPath\**\$projectName.dll`". Maybe an error occurred while building the solution."
            }
            else
            {
                $codeBase = $dllFile | Resolve-Path -Relative
                $genericCodeBase = GetGenericCodeBase $codeBase

                $dllFolder = Split-Path -Parent $codeBase
                $dllFileName = $dllFile.Name
                $fileExtension = Split-Path -Extension $dllFileName

                $isTestDll = ContainsTestPlatformDll $dllFolder

                $info = New-Object -TypeName psobject
                $info | Add-Member -MemberType NoteProperty -Name ProjectName -Value $projectName # The project name without file extension.
                $info | Add-Member -MemberType NoteProperty -Name ProjectFolder -Value $projectFolder # The relative path to the project folder.
                $info | Add-Member -MemberType NoteProperty -Name DllFileName -Value $dllFileName # The name of the DLL or exe file with extension.
                $info | Add-Member -MemberType NoteProperty -Name DllFolder -Value $dllFolder # The relative path to the folder of the DLL or exe file.
                $info | Add-Member -MemberType NoteProperty -Name CodeBase -Value $codeBase # The relative path to DLL or exe file (called CodeBase in trx files).
                $info | Add-Member -MemberType NoteProperty -Name GenericCodeBase -Value $genericCodeBase # The relative path to DLL or exe file with wildcarded framework folder name and without extension.
                $info | Add-Member -MemberType NoteProperty -Name GenericCodeBaseExtension -Value $fileExtension # The extension of the DLL or exe file.
                $info | Add-Member -MemberType NoteProperty -Name TargetFramework -Value $targetFramework # The target framework currently processed by inspecting the according folder in the bin folder.
                $info | Add-Member -MemberType NoteProperty -Name IsTestDll -Value $isTestDll # True, if the DLL/exe is recognized as a test library.

                $dllInfos.Add($info)
                # For exe files a special treatment is needed below - so add them to $exeGenericCodeBases.
                if ($isExeFile)
                {
                    $exeGenericCodeBases.Add($genericCodeBase)
                }
            }
        }
    }

    # Replace GenericCodeBase filename extensions with ".dll|exe" for matches containing DLL and exe.
    # This way we allow grouping by code base for projects that produce a exe file for one target framework and a DLL file for another one.
    foreach ($exeGenericCodeBase in $exeGenericCodeBases)
    {
        $fileMatches = $dllInfos | Where-Object {$_.GenericCodeBase -eq $exeGenericCodeBase}
        $hasDllMatch = ($fileMatches | Where-Object {$_.GenericCodeBaseExtension -eq ".dll"} | Measure-Object | Select-Object -ExpandProperty Count) -gt 0

        if ($hasDllMatch)
        {
            foreach ($match in $fileMatches)
            {
                $match.GenericCodeBaseExtension = ".dll|exe"
            }
        }

    }

    return $dllInfos
}

# Gets the projects of the solution.
function GetSolutionProjects() {
    $entries = dotnet sln $script:Solution list
    $projects = $entries | Where-Object {$_.EndsWith(".csproj")}
    return $projects
}

# Examines if $path contains "xunit.*.dll".
# If so, we can assume that the project DLL in $path contains unit tests to be run.
# This information is needed because starting non test projects in dotnet test may return errors.
function ContainsTestPlatformDll($path) {
    $testDlls = Get-ChildItem -Path $path -Filter "xunit.*.dll" -Recurse -ErrorAction SilentlyContinue -Force
    $result = ($testDlls | Measure-Object | Select-Object -ExpandProperty Count) -gt 0

    return $result
}

# Gets the relative path to DLL or exe file with wildcarded framework folder name and without extension.
function GetGenericCodeBase($codeBase)
{
    $genericCodeBase = $codeBase

    $codeBaseFrameworkPath = Split-Path -Parent $codeBase
    $codeBaseBinPath = Split-Path -Parent $codeBaseFrameworkPath
    $codeBaseBinFolder = Split-Path -Leaf $codeBaseBinPath
    $codeBaseFileNameWithoutExtension = Split-Path -LeafBase $codeBase

    # Wildcard framework folder located in bin folder.
    if ($codeBaseBinFolder.EndsWith($script:Config))
    {
        $genericCodeBase = Join-Path -Path $codeBaseBinPath "*" $codeBaseFileNameWithoutExtension
    }

    return $genericCodeBase
}

# Checks if $checkCodeBase matches $genericCodeBase and $genericCodeBaseExtension
function MatchesGenericCodeBase($checkCodeBase, $genericCodeBase, $genericCodeBaseExtension)
{
    $checkGenericCodeBase = GetGenericCodeBase $checkCodeBase
    $checkExtension = Split-Path -Extension $checkCodeBase

    # If generic code bases match...
    if ($genericCodeBase -eq $checkGenericCodeBase)
    {
        # ... and extension too, return true.
        if ($genericCodeBaseExtension -eq $checkExtension)
        {
            return $true
        }

        # ... and extension not, check if $genericCodeBaseExtension, which may be ".dll|exe", contains $checkExtension and return the result.
        $genericCodeBaseExtensions = $genericCodeBaseExtension -split "|"
        $hasMatch = $genericCodeBaseExtensions | Where-Object {$_.TrimStart(".") -eq $checkExtension.TrimStart(".")}
        return $hasMatch
    }

    return $false
}


# Runs dotnet test for all needed DLL/system combinations.
function RunTests()
{
    if ($script:RunOnWindowsHost)
    {
        RunTestsForSystem $script:TestDllInfosWindows $script:SystemNameWindows $false
    }

    if ($script:RunOnLinuxHost)
    {
        RunTestsForSystem $script:TestDllInfosLinux $script:SystemNameCurrentLinux $false
    }

    if ($script:RunOnHostedWsl)
    {
        RunTestsForSystem $script:TestDllInfosLinux $script:SystemNameCurrentLinux $true
    }
}

# Runs dotnet test for all needed DLLs for the given $systemName.
function RunTestsForSystem($testDllInfos, $systemName, $isHostedWsl)
{
    Write-Output ""
    Write-Output Running tests on $systemName
    Write-Output ==================================================
    foreach ($testDllInfo in $testDllInfos)
    {
        # Work with absolute DLL path here to avoid errors.
        $testDll = Join-Path $testDllInfo.DllFolder $testDllInfo.DllFileName
        $testDllAbsolute = Resolve-Path $testDll

        # For WSL get the corresponding WSL path.
        if ($isHostedWsl)
        {
            # $testDllExecutionPath = wsl wslpath -u $testDllAbsolute
            $testDllExecutionPath = WslPath $testDllAbsolute
        }
        else
        {
            $testDllExecutionPath = $testDllAbsolute
        }

        $TestResultsFilename = GetTestResultsFilename $testDllInfo.EnvironmentName

        Write-Output ""
        Write-Output ($systemName): dotnet test $testDllInfo.DllFileName ("(" + $testDllInfo.TargetFramework + ")")...
        Write-Output ----------------------------------------
        Write-Output ""

        # Change current location to store the trx file in the default "TestResults" folder inside the project folder.
        Push-Location $testDllInfo.ProjectFolder
        try
        {
            # Execute dotnet test on the host or hosted system for the DLL in its target framework.
            if ($isHostedWsl)
            {
                wsl -e dotnet test $testDllExecutionPath --environment $script:EnvironmentForHostedWsl -l "trx;LogFileName=./$TestResultsFilename" --framework $testDllInfo.TargetFramework
            }
            else
            {
                dotnet test $testDllExecutionPath -l "trx;LogFileName=./$TestResultsFilename" --framework $testDllInfo.TargetFramework
            }
            RestoreForegroundColor # The dotnet call may change the foreground color, e. g. when displaying test result exceptions.
        }
        finally
        {
            Pop-Location
        }
    }
    Write-Output ""
    Write-Output ""
}

# Gets the name of the environment for the given system and framework.
function GetEnvironmentName($systemName, $targetFramework)
{
    if ($script:Net10 -and $targetFramework.Contains("net10"))
    {
        $frameworkName = $script:NetName10
    }
    elseif ($script:Net10 -eq $false -and $targetFramework.Contains("net8"))
    {
        $frameworkName = $script:NetName8
    }
    elseif ($targetFramework.Contains("net462"))
    {
        $frameworkName = $script:NetName462
    }
    else
    {
        Write-Error ("Framework `"" + $targetFramework + "`" is not yet supported by test script.")
    }

    # HACK: For Linux the frameworkName shall not be shown.
    if ($systemName -eq $script:SystemNameCurrentLinux)
    {
        if ($script:Net10 -and $frameworkName -ne "net10")
        {
            Write-Error ("For Linux there’s only one column supported (net10) by test script with Net10 parameter set to true.")
        }
        elseif ($script:Net10 -eq $false -and $frameworkName -ne "net8")
        {
            Write-Error ("For Linux there’s only one column supported (net8) by test script with Net10 parameter set to false.")
        }
        return "$systemName"
    }

    return "$systemName-$frameworkName"
}

# Gets the Windows or Linux DllInfos list according to the environment name.
function GetDllInfosByEnvironmentName($environmentName)
{
    # Get the system specific $dllInfos for this environment.
    if ($environmentName.StartsWith($script:SystemNameWindows))
    {
        $dllInfos = $script:TestDllInfosWindows
    }
    elseif ($environmentName.StartsWith($script:SystemNameCurrentLinux))
    {
        $dllInfos = $script:TestDllInfosLinux
    }
    else
    {
        Write-Error "Could not determine environment system for `"$environmentName`""
    }

    return $dllInfos
}

# Gets the name of the trx file for the given environment.
function GetTestResultsFilename($environmentName)
{
    $TestResultsFilename = "test-$environmentName.trx"
    return $TestResultsFilename
}


# Loads the trx files and displays the test results.
function LoadAndShowTestResults()
{
    # TODO  -ForegroundColor Green # Green color is used to make it the same conspicuity like the given green Format-Table header output.
    Write-Output ""
    Write-Output ""
    Write-Output "TestResults"
    Write-Output "=================================================="

    if ($script:Net10)
    {
        $netNameX = $script:NetName10
    }
    else
    {
        $netNameX = $script:NetName8
    }

    # Environment names to be displayed in separate columns.
    $environmentNameWindowsNetX = GetEnvironmentName $script:SystemNameWindows $netNameX
    $environmentNameWindowsNet462 = GetEnvironmentName $script:SystemNameWindows $script:NetName462
    $environmentNameLinuxNetX = GetEnvironmentName $script:SystemNameCurrentLinux $netNameX

    $environmentNames = @($environmentNameWindowsNetX, $environmentNameWindowsNet462, $environmentNameLinuxNetX)

    # Get unique GenericCodeBaseinformation for all Windows and Linux test DLLs.
    $genericCodeBaseInfos = ($script:TestDllInfosWindows + $script:TestDllInfosLinux) | Select-Object -Property GenericCodeBase, GenericCodeBaseExtension, ProjectFolder -Unique


    # Define formats for Format-Table
    $totalWidth = $script:ConsoleWidth - 2 # Subtract 2 characters of width Format-Table will not use.

    # Define the widths of the result columns
    $resultColumnWidth = 17
    $firstResultColumnLeftPadding = 2 # The test column wraps at the first result column with almost no padding. Add a manual left padding for the first result column as there is no way to define column padding.
    $firstResultColumnLeftPaddingStr = " " * $firstResultColumnLeftPadding

    # The test column gets the rest of the available width.
    $testColumnWidth = $totalWidth - $firstResultColumnLeftPadding - 3 * $resultColumnWidth

    # Cannot generate $formats automatically, as Expression is executed later.
    # In a loop generating the formats, all Expressions would get only the last assigned value of the loop variable.
    $formats = @(
        @{
            Label = "Test"
            Expression = { $_.TestName }
            Width = $testColumnWidth
            },
        @{
            Label = $firstResultColumnLeftPaddingStr + "$environmentNameWindowsNetX"
            Expression = { ColorizedCellFormatExpressionResult($firstResultColumnLeftPaddingStr + $_.($environmentNameWindowsNetX)) }
            Width = $firstResultColumnLeftPadding + $resultColumnWidth
        },
        @{
            Label = "$environmentNameWindowsNet462"
            Expression = { ColorizedCellFormatExpressionResult($_.($environmentNameWindowsNet462)) }
            Width = $resultColumnWidth
        },
        @{
            Label = "$environmentNameLinuxNetX"
            Expression = { ColorizedCellFormatExpressionResult($_.($environmentNameLinuxNetX)) }
            Width = $resultColumnWidth
        }
    )


    # Load and group all test result.
    $allGroupedResults = @() # Collects all results grouped by test names.

    # Use a padding to display CodeBase and the environment’s trx files.
    $padValue = (($environmentNames | Select-Object -ExpandProperty Length | Measure-Object -Maximum).Maximum)

    # Loop all GenericCodeBases. For each GenericCodeBase a separate table containing the according test results is outputted.
    foreach ($genericCodeBaseInfo in $genericCodeBaseInfos)
    {
        $genericCodeBase = $genericCodeBaseInfo.GenericCodeBase

        $genericCodeBaseWithExtension = $genericCodeBaseInfo.GenericCodeBase + $genericCodeBaseInfo.GenericCodeBaseExtension

        # TODO  -ForegroundColor Green # Green color is used to make it the same conspicuity like the given green Format-Table header output.
        Write-Output ""
        Write-Output ""
        $title = ("CodeBase:").PadRight($padValue + 1, ' ')
        Write-Output $title $genericCodeBaseWithExtension
        Write-Output "----------------------------------------------------------------------------------------------------"
        Write-Output Test files:


        $codeBaseResults = @() # Collects all results for this GenericCodeBase.
        $testResultsFiles = @() # Collects information for the environment specific trx files for this GenericCodeBase.

        # Loop environments to load the according trx file for the code base.
        foreach ($environmentName in $environmentNames)
        {
            $environmentResults = @() # Collects all results for this GenericCodeBase and environment.

            $testResultsFilename = GetTestResultsFilename $environmentName
            $testResultsFile = Join-Path -Path $genericCodeBaseInfo.ProjectFolder "TestResults" $testResultsFilename
            $testResultsFileExists = Test-Path $testResultsFile
            $testResultsFileOutdated = $testResultsFileExists -and (Get-Item $testResultsFile).LastWriteTime -le $script:TimeStart

            $dllInfos = GetDllInfosByEnvironmentName $environmentName

            $referringDllInfos = $dllInfos | Where-Object GenericCodeBase -eq $genericCodeBase;

            $notTargetingEnvironment = ($referringDllInfos | Where-Object EnvironmentName -eq $environmentName | Measure-Object | Select-Object -ExpandProperty Count) -eq 0

            $title = ($environmentName + ":").PadRight($padValue + 1, ' ')
            # If the project is not targeting this environment.
            if ($notTargetingEnvironment)
            {
                Write-Output $title Not implemented.
            }
            # If trx file is not found or outdated, output "No test results found" and continue loop.
            elseif ($testResultsFileExists -eq $false -or $testResultsFileOutdated)
            {
                $testResultsFile = $null
                Write-Output $title No test results found.
            }
            # If trx file is found and from this test run, collect results from the file.
            else
            {
                Write-Output $title $testResultsFile

                # Read trx content.
                [xml]$fileContent = Get-Content -Path $testResultsFile
                $nameSpace = "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"
                $nameSpaceManager = new-object Xml.XmlNamespaceManager $fileContent.NameTable
                $nameSpaceManager.AddNamespace("ns", $nameSpace)

                # Get all test definitions from trx file.
                $testDefinitions = $fileContent.SelectNodes("//ns:UnitTest/ns:Execution", $nameSpaceManager)

                # Get all unique code bases from test definitions.
                $codeBases = $testDefinitions | ForEach-Object {
                    $value = $_.NextSibling.codeBase
                    # If started in Windows convert path to Windows path. This way even the results of hosted WSL test get the Windows code bases to provide a uniform output.
                    if ($script:RunOnWindowsHost)
                    {
                        $value = GetWindowsPath $value
                    }
                    return $value | Resolve-Path -Relative
                } | Select-Object -Unique

                # Check if all code bases in the trx file match the GenericCodeBase the trx file was found for.
                foreach ($codeBase in $codeBases)
                {
                    $matchesGenericCodeBase = MatchesGenericCodeBase $codeBase $genericCodeBase $genericCodeBaseInfo.GenericCodeBaseExtension
                    if ($matchesGenericCodeBase -eq $false)
                    {
                        Write-Error "Differing CodeBase found in `"$testResultsFile`": $codeBase"
                    }
                }

                # Add test results from the trx file.
                foreach ($unitTestResult in $fileContent.SelectNodes('//ns:UnitTestResult', $nameSpaceManager))
                {
                    $testResult = New-Object -TypeName PsObject

                    $testResult | Add-Member -MemberType NoteProperty -Name ExecutionId -Value $unitTestResult.executionId
                    $testResult | Add-Member -MemberType NoteProperty -Name TestName -Value $unitTestResult.testName
                    $testResult | Add-Member -MemberType NoteProperty -Name Outcome -Value $unitTestResult.outcome # The test result.

                    # Not used:
                    #$testResult | Add-Member -MemberType NoteProperty -Name Duration -Value $unitTestResult.duration
                    #$testResult | Add-Member -MemberType NoteProperty -Name Message -Value $unitTestResult.Output.ErrorInfo.Message
                    #$testResult | Add-Member -MemberType NoteProperty -Name StackTrace -Value $unitTestResult.Output.ErrorInfo.StackTrace

                    $testResult | Add-Member -MemberType NoteProperty -Name Environment -Value $environmentName

                    # Save the GenericCodeBase instead of the not generic code base value from the trx file in the test result object to make the test results comparable and groupable.
                    $testResult | Add-Member -MemberType NoteProperty -Name GenericCodeBase -Value $genericCodeBase

                    # Add test result for this environment.
                    $environmentResults += $testResult
                }
            }
            # Add environment test results for this GenericCodeBase.
            $codeBaseResults += $environmentResults

            # Add information for trx file.
            $testResultsFileData = New-Object -TypeName PsObject
            $testResultsFileData | Add-Member -MemberType NoteProperty -Name Environment -Value $environmentName
            $testResultsFileData | Add-Member -MemberType NoteProperty -Name TestResultsFile -Value $testResultsFile
            $testResultsFileData | Add-Member -MemberType NoteProperty -Name NotTargetingEnvironment -Value $notTargetingEnvironment
            $testResultsFiles += $testResultsFileData
        }

        # Group test results for the GenericCodeBase by test name.
        $groupedResults = GroupTestResults $codeBaseResults $environmentNames $testResultsFiles $genericCodeBaseInfo.GenericCodeBase

        # Output grouped GenericCodeBase test results.
        $groupedResults | Format-Table $formats -Wrap

        # Add grouped test results for this GenericCodeBase.
        $allGroupedResults += $groupedResults
    }


    # Add summary for all grouped test results.
    # TODO  -ForegroundColor Green # Green color is used to make it the same conspicuity like the given green Format-Table header output.
    Write-Output ""
    Write-Output ""
    Write-Output ""
    Write-Output Summary
    Write-Output "----------------------------------------------------------------------------------------------------"

    # Create test summary from grouped test results.
    $summary = CreateTestSummary $allGroupedResults $environmentNames

    # Change test column header for summary output.
    $formats[0].Label = "Total:"

    # Output test results summary.
    $summary | Format-Table $formats -Wrap
}

# Gets the Windows path according to a WSL path.
function GetWindowsPath($path) {
    if ($path -match "^/mnt/(?<drive>\w)/")
    {
        #$path = wsl -e wslpath -w $path
        # Do manual folder conversion as calling wslpath is very very slow.
        $path = -join($Matches.drive.ToUpper(), ":\", $path.Substring(7).Replace('/', '\'))
    }
    return $path
}

# Group test results by test name.
function GroupTestResults($testResults, $environmentNames, $testResultsFiles, $genericCodeBase)
{
    $groupedResults = @() # Collects all test results grouped by test name.

    # Group test results by test name.
    $testNameGroups = $testResults | Group-Object TestName | Sort-Object Name

    # For each test name...
    foreach ($testNameGroup in $testNameGroups)
    {
        # ...create an object containing the test name...
        $groupedResult = New-Object -TypeName PsObject
        $groupedResult | Add-Member -MemberType NoteProperty -Name TestName -Value $testNameGroup.Name

        # ...and a member for each environment.
        foreach ($environmentName in $environmentNames)
        {
            $dllInfos = GetDllInfosByEnvironmentName $environmentName

            # If the GenericCodeBase, these test results belong to, is not found in this system $dllInfos, the GenericCodeBase is not applicable for this environment (they where intended not to run).
            $isNotApplicable = ($dllInfos | Where-Object GenericCodeBase -eq $genericCodeBase | Measure-Object | Select-Object -ExpandProperty Count) -eq 0

            # If the GenericCodeBase is not applicable, $outcome shall be "not applicable".
            if ($isNotApplicable)
            {
                # DLL was not included in environment TestDllInfos.
                $outcome = "Not applicable"
            }
            else
            {
                # Get the test result of this group (test name) for this environment.
                $testResult = $testNameGroup.Group | Where-Object Environment -eq $environmentName | Select-Object -First 1

                # If the GenericCodeBase is applicable, but no test result is found...
                if ($null -eq $testResult)
                {
                    $testResultsFile = $testResultsFiles | Where-Object Environment -eq $environmentName | Select-Object -First 1
                    $hasTrxFile = $null -ne $testResultsFile.TestResultsFile

                    # ...the project may not be targeting the environment or...
                    if ($testResultsFile.NotTargetingEnvironment)
                    {
                        $outcome = "Not implemented"
                    }
                    # ...the test is not found in the trx file, so we assume that it’s only implemented for other environments or...
                    elseif ($hasTrxFile)
                    {
                        $outcome = "Not implemented"
                    }
                    # ...the trx file is not found, although the project is targeting the environment. Is there an error?
                    else
                    {
                        $outcome = "No trx file"
                    }
                }
                # If the GenericCodeBase is applicable, and the test result is found, set $outcome shall be taken from the test result.
                else
                {
                    $outcome = $testResult.Outcome

                    # For "NotExecuted" "Skipped" shall be returned. Attention: Maybe other circumstances than "Skipped" could result in "NotExecuted" in the trx file.
                    if ($outcome -eq "NotExecuted")
                    {
                        $outcome = "Skipped"
                    }
                    elseif ($null -eq $outcome)
                    {
                        $outcome = "???"
                    }
                }
            }

            # For this environment add the calculated $outcome / test result.
            $groupedResult | Add-Member -MemberType NoteProperty -Name $environmentName -Value $outcome
        }

        $groupedResults += $groupedResult
    }

    return $groupedResults
}

# Creates a test summary from the grouped test results.
function CreateTestSummary($allGroupedResults, $environmentNames)
{
    $summary = @()

    $passedLine = New-Object -TypeName PsObject
    foreach ($environmentName in $environmentNames)
    {
        $count = ($allGroupedResults | Where-Object $environmentName -EQ "Passed" | Measure-Object).Count
        $passedLine | Add-Member -MemberType NoteProperty -Name $environmentName -Value "Passed:    $count"
    }
    $summary += $passedLine


    $failedLine = New-Object -TypeName PsObject
    foreach ($environmentName in $environmentNames)
    {
        $count = ($allGroupedResults | Where-Object $environmentName -EQ "Failed" | Measure-Object).Count
        $failedLine | Add-Member -MemberType NoteProperty -Name $environmentName -Value "Failed:    $count"
    }
    $summary += $failedLine


    $notImplementedLine = New-Object -TypeName PsObject
    foreach ($environmentName in $environmentNames)
    {
        $count = ($allGroupedResults | Where-Object $environmentName -EQ "Not implemented" | Measure-Object).Count
        $notImplementedLine | Add-Member -MemberType NoteProperty -Name $environmentName -Value "Not impl:  $count"
    }
    $summary += $notImplementedLine

    return $summary
}

# Gets the expression for Format-Table $format, that formats $value in value-dependent color.
function ColorizedCellFormatExpressionResult($value)
{
    # Try to extract a count integer value from $value.
    $split = $value.Split()
    $count = $null;
    if ($split.Length -ge 2) {
        $split2 = $split[$split.Length - 1]
        $parsedCount = 0;
        if (([System.Int32]::TryParse($split2, [ref] $parsedCount))) {
            $count = $parsedCount
        }
    }

    # Color codes see https://duffney.io/usingansiescapesequencespowershell/#basic-foreground-background-colors
    # Noticeable default value that should not occur.
    $color = "33" # Orange

    # White color for "not applicable" and summary lines with a count of 0.
    if ($count -eq 0 -or $value.ToLower().Contains("not applicable"))
    {
        $color = "37" # White
    }
    elseif ($value.ToLower().Contains("failed"))
    {
        $color = "31" # Red
    }
    elseif ($value.ToLower().Contains("passed"))
    {
        $color = "32" # Green
    }

    # Magic to output $value in the color.
    $e = [char]27
    "$e[${color}m$($value)${e}[0m"
}

function SaveForegroundColor()
{
    $script:foregroundColorBackup = $host.UI.RawUI.ForegroundColor
}

function RestoreForegroundColor()
{
    $host.UI.RawUI.ForegroundColor = $script:foregroundColorBackup
}


<#
.SYNOPSIS
Converts Windows path into a Linux path and vice versa.
.DESCRIPTION
Converts Windows path into a Linux path and vice versa. Uses WSL under the hood, so needs to be installed.
See wslpath docs for more information.
.PARAMETER path
The path to convert.
.PARAMETER conversion
The direction of conversion Windows->Linux by default ('-u'). See wslpath docs for other options.
.EXAMPLE
wslpath $Profile
wslpath $Profile '-w'
#>
function WslPath(
    [Parameter(Mandatory)]
    [string]
    $path,

    [ValidateSet('-u', '-w', '-m')]
    $conversion = '-u'
)
{
    wsl 'wslpath' $conversion $path.Replace('\', '\\');
}


# ***** Main *****

Push-Location $PSScriptRoot
try
{
    # Execute script content in the parent folder of the script root.
    Push-Location ..
    try
    {
        InitializeScript

        # Test-HACK for loading all found test result files and not only the ones created in this run.
        #$script:TimeStart = Get-Date -Year 1900

        BuildSolution

        LoadTestDllInfos

        RunTests

        LoadAndShowTestResults
    }
    finally
    {
        Pop-Location
    }
}
finally
{
    Pop-Location
}