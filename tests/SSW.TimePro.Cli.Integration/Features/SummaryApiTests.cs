using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace SSW.TimePro.Cli.Integration.Features;

public class SummaryApiTests : TestBase
{
    [Fact]
    public async Task GetProjectsSummary_SendsServerControlledWindowQuery()
    {
        WireMock.Given(
            Request.Create()
                .WithPath("/api/Timesheets/GetProjectsSummary")
                .UsingGet()
        ).RespondWith(
            Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[]")
        );

        var result = await ApiClient.GetProjectsSummaryAsync(
            "TST",
            new DateOnly(2026, 3, 1),
            new DateOnly(2026, 3, 31),
            CancellationToken.None);

        result.Should().BeEmpty();
        var request = WireMock.LogEntries.Single().RequestMessage!;
        var url = request.Url!.ToString();
        url.Should().Contain("employeeID=TST");
        url.Should().Contain("currentDate=2026-03-31");
        url.Should().NotContain("startDate=");
        url.Should().NotContain("endDate=");
    }
}
