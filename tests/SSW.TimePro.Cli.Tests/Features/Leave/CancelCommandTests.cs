using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SSW.TimePro.Cli.Infrastructure.ApiClient;
using SSW.TimePro.Cli.Infrastructure.DependencyInjection;
using Spectre.Console.Cli;
using Xunit;

using LeaveCancelCommand = SSW.TimePro.Cli.Features.Leave.CancelCommand;

namespace SSW.TimePro.Cli.Tests.Features.Leave;

public class CancelCommandTests
{
    [Fact]
    public async Task Cancel_WhenLeaveIdIsNotGuid_ReturnsErrorWithoutCallingApi()
    {
        var api = Substitute.For<ITimeProApiClient>();
        var app = CreateApp(api);

        var exitCode = await app.RunAsync([
            "cancel",
            "not-a-guid",
            "--reason", "Plans changed",
            "--json"
        ], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        api.ShouldNotHaveReceived(nameof(ITimeProApiClient.CancelLeaveAsync));
    }

    [Fact]
    public async Task Cancel_WhenReasonMissing_ReturnsErrorWithoutCallingApi()
    {
        var api = Substitute.For<ITimeProApiClient>();
        var app = CreateApp(api);

        var exitCode = await app.RunAsync([
            "cancel",
            "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
            "--json"
        ], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        api.ShouldNotHaveReceived(nameof(ITimeProApiClient.CancelLeaveAsync));
    }

    private static CommandApp CreateApp(ITimeProApiClient api)
    {
        var services = new ServiceCollection();
        services.AddSingleton(api);

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config =>
        {
            config.AddCommand<LeaveCancelCommand>("cancel");
        });
        return app;
    }
}
