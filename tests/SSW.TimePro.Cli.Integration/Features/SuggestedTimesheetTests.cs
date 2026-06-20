using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace SSW.TimePro.Cli.Integration.Features;

public class SuggestedTimesheetTests : TestBase
{
    [Fact]
    public async Task RefreshSuggestedTimesheets_WhenResponseBodyIsEmpty_ReturnsEmptyList()
    {
        WireMock.Given(
            Request.Create()
                .WithPath("/api/Timesheets/RefreshSuggestedTimesheets")
                .WithParam("employeeID", "TST")
                .WithParam("timesheetDate", "2026-03-30T00:00:00")
                .UsingGet()
        ).RespondWith(
            Response.Create()
                .WithStatusCode(200)
        );

        var result = await ApiClient.RefreshSuggestedTimesheetsAsync(
            "TST",
            new DateOnly(2026, 3, 30),
            CancellationToken.None);

        result.Should().BeEmpty();
    }
}
