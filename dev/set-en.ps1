# Sets UI language for dotnet build to en-US.
#Requires -Version 7
#Requires -PSEdition Core

# I want to see English messages on my German Windows dev machine
# when I run e.g. dotnet build.

# What works:
$env:VSLANG = 1033
Write-Output "Compiler output language set to en-US."

# What does not work:
#$env:DOTNET_CLI_UI_LANG = 'en-US'
#$env:PreferredUILang = 'en-US'
#powershell -ExecutionPolicy Bypass -NoExit -Command "[System.Threading.Thread]::CurrentThread.CurrentCulture = 'en-US'; [System.Threading.Thread]::CurrentThread.CurrentUICulture = 'en-US';"
#powershell -ExecutionPolicy Bypass -NoExit -Command "[System.Threading.Thread]::CurrentThread.CurrentCulture = 'en-US'; [System.Threading.Thread]::CurrentThread.CurrentUICulture = 'en-US';"
