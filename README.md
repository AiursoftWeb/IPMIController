# Aiursoft IPMIController

<a name="ipmicontroller"></a>
[![MIT licensed](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/AiursoftWeb/ipmicontroller/blob/master/LICENSE)
[![Pipeline stat](https://badge-proxy.aiursoft.com/aiursoft/IPMIController/badges/master/pipeline.svg)](https://gitlab.aiursoft.com/aiursoft/IPMIController/-/pipelines)
[![Test Coverage](https://badge-proxy.aiursoft.com/aiursoft/IPMIController/badges/master/coverage.svg)](https://gitlab.aiursoft.com/aiursoft/IPMIController/-/pipelines)
[![S360 compliance](https://s360.aiursoft.com/badge.svg?project=aiursoft%2Fipmicontroller)](https://s360.aiursoft.com/SupplyChain?tab=projects&project=aiursoft%2Fipmicontroller&scope=baseline&ecosystem=all)
[![NuGet version (Aiursoft.IPMIController)](https://img.shields.io/nuget/v/Aiursoft.IPMIController.svg)](https://www.nuget.org/packages/Aiursoft.IPMIController/)
[![Man hours](https://manhours.aiursoft.com/r/github.com/aiursoftweb/IPMIController.svg)](https://manhours.aiursoft.com/r/github.com/aiursoftweb/IPMIController.html)

IPMI Controller is a .NET based CLI tool to control the server fan via IPMI. (Tested with Dell iDrac)

## Overview

### What is IPMIController?

Monitor server temperatures and manage fan speed through IPMI, with optional ESXi GPU temperature input. Requires ipmitool and access to the configured servers.

## Installation

Install the CLI with the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0):

```bash
sudo apt install ipmitool
dotnet tool install --global Aiursoft.IPMIController
```

Update an existing installation:

```bash
dotnet tool update --global Aiursoft.IpmiController
```

Remove the CLI:

```bash
dotnet tool uninstall --global Aiursoft.IpmiController
```

## Usage

Run the command to see available options:

```bash
ipmi-controller --help
```

<a name="how-to-use"></a>
It will read the `Servers` array and try to connect to each server. If the connection is successful, it will try to read the current fan speed and then set the fan speed to the target speed.

```bash
$ ipmi-controller --profile quiet
Description:
  Monitor the temperature of your servers.

Usage:
  ipmi-controller [options]

Options:
  -p, --profile <profile>  The target profile. Can be: 'auto','turbo','normal','quiet','full'. [default: auto]
  -v, --verbose            Show detailed log
  -?, -h, --help           Show help and usage information
```

## Configuration

It requires the current directory to have a `appsettings.json` file.

```json
{
  "Servers": [
    {
      "HostOrIp": "10.0.0.1",
      "RootPassword": "pass@word1",
      "EsxiIp": "10.0.0.100",
      "EsxiRootPassword": "pass@word2",
      "Offset": 0
    }
  ]
}
```

* HostOrIp must be the IPMI address of the server.
* RootPassword must be the IPMI password of the server.
* EsxiIp is optional. If provided, it will try to connect to the ESXi server to get the `nvidia-smi` temperature.
* ExsiRootPassword is **optional**. If provided, it will try to connect to the ESXi server to get the `nvidia-smi` temperature.
* Offset is **optional**. If provided, it will add the offset to the target temperature.
  * For example, if `Offset` is 10, then a 30-degree target temperature will be treated as 40 degrees.

**Command-line options**

`--profile` (`-p`) selects `auto`, `turbo`, `normal`, `quiet` or `full` (default `auto`). `--minfan` (`-m`) sets the minimum fan speed in percent (default `6`). `--verbose` enables detailed logging. Read current help with `ipmi-controller --help`. The current single-command entry point does not register a `--dry-run` option.

Protect `appsettings.json`: it contains the credentials used to access the configured IPMI and optional ESXi endpoints.

## Development

Install the [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0).

From the repository root, restore and build:

```bash
dotnet restore Aiursoft.IPMIController.sln
dotnet build Aiursoft.IPMIController.sln --no-restore
```

<a name="run-locally"></a>
<a name="local-run"></a>
Run the tool from source, with the prerequisites and configuration above:

```bash
dotnet run --project src/Aiursoft.IpmiController/Aiursoft.IpmiController.csproj -- --help
```

<a name="run-in-microsoft-visual-studio"></a>
In Microsoft Visual Studio, open `Aiursoft.IPMIController.sln`, select `Aiursoft.IpmiController` as the startup project and press `F5`. Configure the arguments and environment for the task you want to run.

Run the existing tests:

```bash
dotnet test Aiursoft.IPMIController.sln --no-restore
```


## How to contribute

There are many ways to contribute to the project: logging bugs, submitting pull requests, reporting issues, and creating suggestions.

Even if you have push rights on the repository, you should create a personal fork and create feature branches there when you need them. This keeps the main repository clean and your workflow cruft out of sight.

We're also interested in your feedback on the future of this project. You can submit a suggestion or feature request through the issue tracker. To make this process more effective, we're asking that these include more information to help define them more clearly.

## License

Licensed under the [MIT License](https://github.com/AiursoftWeb/ipmicontroller/blob/master/LICENSE).
