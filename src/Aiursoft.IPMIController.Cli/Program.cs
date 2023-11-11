// Program.cs
using Aiursoft.CommandFramework;
using Aiursoft.CommandFramework.Extensions;

return await new AiursoftCommand()
    .Configure(command =>
    {
        command
            .AddGlobalOptions()
            .AddPlugins(new MonitorPlugin());
    })
    .RunAsync(args);