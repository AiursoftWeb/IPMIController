﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Aiursoft.CommandFramework;
using Aiursoft.CommandFramework.Extensions;
using Aiursoft.IpmiController.Services;

namespace Aiursoft.IpmiController.Tests;

[TestClass]
public class IntegrationTests
{
    private readonly AiursoftCommand _program;

    public IntegrationTests()
    {
        _program = new AiursoftCommand()
            .Configure(command =>
            {
                command
                    .AddGlobalOptions()
                    .AddPlugins(
                        new MonitorPlugin()
                    );
            });
    }

    [TestMethod]
    public async Task InvokeHelp()
    {
        var result = await _program.TestRunAsync(new[] { "--help" });

        Assert.AreEqual(0, result.ProgramReturn);
        Assert.IsTrue(result.Output.Contains("Options:"));
        Assert.IsTrue(string.IsNullOrWhiteSpace(result.Error));

    }

    [TestMethod]
    public async Task InvokeVersion()
    {
        var result = await _program.TestRunAsync(new[] { "--version" });
        Assert.AreEqual(0, result.ProgramReturn);
    }

    [TestMethod]
    public async Task InvokeUnknown()
    {
        var result = await _program.TestRunAsync(new[] { "--wtf" });
        Assert.AreEqual(1, result.ProgramReturn);
    }

    [TestMethod]
    public async Task InvokeWithoutArg()
    {
        var result = await _program.TestRunAsync(Array.Empty<string>());
        Assert.AreEqual(1, result.ProgramReturn);
    }

}