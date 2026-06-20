using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SSW.TimePro.Cli.Infrastructure.ApiClient;
using SSW.TimePro.Cli.Infrastructure.Config;
using SSW.TimePro.Cli.Infrastructure.DependencyInjection;
using Spectre.Console.Cli;
using Xunit;

using LeaveCreateCommand = SSW.TimePro.Cli.Features.Leave.CreateCommand;

namespace SSW.TimePro.Cli.Tests.Features.Leave;

public class CreateCommandTests
{
    [Fact]
    public async Task Create_WhenNoteMissing_ReturnsErrorWithoutCallingApi()
    {
        var api = Substitute.For<ITimeProApiClient>();
        var app = CreateApp(api);

        var exitCode = await app.RunAsync([
            "create",
            "--start", "2026-03-30",
            "--end", "2026-03-30",
            "--type", "1",
            "--json"
        ], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        api.ShouldNotHaveReceived(nameof(ITimeProApiClient.CreateLeaveAsync));
        api.ShouldNotHaveReceived(nameof(ITimeProApiClient.GetLeaveTypesAsync));
    }

    [Fact]
    public async Task Create_WhenNoteWhitespace_ReturnsErrorWithoutCallingApi()
    {
        var api = Substitute.For<ITimeProApiClient>();
        var app = CreateApp(api);

        var exitCode = await app.RunAsync([
            "create",
            "--start", "2026-03-30",
            "--end", "2026-03-30",
            "--type", "1",
            "--note", "   ",
            "--json"
        ], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        api.ShouldNotHaveReceived(nameof(ITimeProApiClient.CreateLeaveAsync));
        api.ShouldNotHaveReceived(nameof(ITimeProApiClient.GetLeaveTypesAsync));
    }

    [Fact]
    public async Task Create_WhenStartDateIsWeekend_ReturnsErrorWithoutCallingApi()
    {
        var api = Substitute.For<ITimeProApiClient>();
        var app = CreateApp(api);

        var exitCode = await app.RunAsync([
            "create",
            "--start", "2026-03-28",
            "--end", "2026-03-30",
            "--type", "1",
            "--note", "Annual leave",
            "--json"
        ], TestContext.Current.CancellationToken);

        exitCode.Should().Be(1);
        api.ShouldNotHaveReceived(nameof(ITimeProApiClient.CreateLeaveAsync));
        api.ShouldNotHaveReceived(nameof(ITimeProApiClient.GetLeaveTypesAsync));
    }

    private static CommandApp CreateApp(ITimeProApiClient api)
    {
        var tenantProvider = Substitute.For<ITenantProvider>();
        tenantProvider.GetCurrentTenant().Returns(new TenantConfig
        {
            TenantId = "test",
            ApiUrl = "https://timepro.example",
            ApiKey = "test-api-key",
            EmployeeId = "TST"
        });

        var services = new ServiceCollection();
        services.AddSingleton(api);
        services.AddSingleton(tenantProvider);

        var app = new CommandApp(new TypeRegistrar(services));
        app.Configure(config =>
        {
            config.AddCommand<LeaveCreateCommand>("create");
        });
        return app;
    }
}

internal static class SubstituteApiAssertions
{
    public static void ShouldNotHaveReceived(this ITimeProApiClient api, string methodName)
    {
        api.ReceivedCalls()
            .Should()
            .NotContain(call => call.GetMethodInfo().Name == methodName);
    }
}
