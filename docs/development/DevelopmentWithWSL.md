# Development with WSL

## Setup WSL

Follow the instructions from Microsoft to set up WSL:  
https://learn.microsoft.com/en-us/windows/wsl/install

## Run and debug projects in WSL

To run and debug projects, follow the instructions from Microsoft to debug .NET apps in WSL:  
https://learn.microsoft.com/en-us/visualstudio/debugger/debug-dotnet-core-in-wsl-2  
It requires you to set up a launchSettings.json in the properties folder of the project, which could look like this:

```json
{
  "profiles": {
    "Windows": {
      "commandName": "Project"
    },
    "WSL": {
      "commandName": "WSL",
      "distributionName": ""
    }
  }
}
```

Afterwards, you can choose the profile when starting the project.


## Testing in WSL

### Run tests in Visual Studio

To run tests from Visual Studio in your WSL, follow Microsoft’s instructions to enable remote testing:  
https://aka.ms/remotetesting  
It requires you to set up a testenvironments.json in the root of the solution, which could look like this:

```json
{
  "version": "1",
  "environments": [
    // See https://aka.ms/remotetesting for more details
    // about how to configure remote environments.
    {
      "name": "WSL Ubuntu",
      "type": "wsl",
      "wslDistribution": "Ubuntu"
    }
  ]
}
```

Afterwards, you can choose the test environment in Test Explorer.

### Run run-tests script

With the run-test.ps1 in the dev folder to run all tests in in all available environments, including WSL.
For more information run `help run-tests.ps1`.
