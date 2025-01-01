# IPMIController

[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://gitlab.aiursoft.cn/aiursoft/IPMIController/-/blob/master/LICENSE)
[![Pipeline stat](https://gitlab.aiursoft.cn/aiursoft/IPMIController/badges/master/pipeline.svg)](https://gitlab.aiursoft.cn/aiursoft/IPMIController/-/pipelines)
[![Test Coverage](https://gitlab.aiursoft.cn/aiursoft/IPMIController/badges/master/coverage.svg)](https://gitlab.aiursoft.cn/aiursoft/IPMIController/-/pipelines)
[![NuGet version (Aiursoft.IPMIController)](https://img.shields.io/nuget/v/Aiursoft.IPMIController.svg)](https://www.nuget.org/packages/Aiursoft.IPMIController/)
[![ManHours](https://manhours.aiursoft.cn/r/gitlab.aiursoft.cn/aiursoft/IPMIController.svg)](https://gitlab.aiursoft.cn/aiursoft/IPMIController/-/commits/master?ref_type=heads)

IPMI Controller is a .NET based CLI tool to control the server fan via IPMI. (Tested with Dell iDrac)

## Install

Requirements:

1. [.NET 9 SDK](http://dot.net/)

Run the following command to install this tool:

```bash
sudo apt install ipmitool
dotnet tool install --global Aiursoft.IPMIController
```

## How to use

It requires the current directory to have a `appsettings.json` file.

```json
{
  "Servers": [
    {
      "HostOrIp": "10.0.0.1",
      "RootPassword": "pass@word1"
    }
  ]
}
```

It will read the `Servers` array and try to connect to each server. If the connection is successful, it will try to read the current fan speed and then set the fan speed to the target speed.

```bash
$ ipmi-controller monitor --profile quiet
Description:
  Monitor the temperature of your servers.

Usage:
  ipmi-controller monitor [options]

Options:
  -p, --profile <profile>  The target profile. Can be: 'auto','turbo','normal','quiet','full'. [default: auto]
  -d, --dry-run            Preview changes without actually making them
  -v, --verbose            Show detailed log
  -?, -h, --help           Show help and usage information
```

## How to run from source code

Requirements about how to run

1. [.NET 9 SDK](http://dot.net/)
2. Execute `dotnet run` to run the app

## Run in Microsoft Visual Studio

1. Open the `.sln` file in the project path.
2. Press `F5`.

## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you with push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your workflow cruft out of sight.

We're also interested in your feedback on the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.