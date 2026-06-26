using System.Text.Json;
using FluentAssertions;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;

namespace SSW.TimePro.Cli.Integration.Features;

public class UserApiTests : TestBase
{
    [Fact]
    public async Task ListUsers_HydratesDropdownIdsWithEmail()
    {
        WireMock.Given(Request.Create()
                .WithPath("/api/Employees/DropDown")
                .WithParam("includeExEmployees", "false")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                [
                  { "Text": "Test User (TST)", "Value": "TST" },
                  { "Text": "Alex Example (AEX)", "Value": "AEX" }
                ]
                """));

        WireMock.Given(Request.Create()
                .WithPath("/api/Employees/GetByIds")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                [
                  { "EmpID": "AEX", "Name": "Alex Example", "Email": "alex.example@example.com" },
                  { "EmpID": "TST", "Name": "Test User", "Email": "test.user@example.com" }
                ]
                """));

        var users = await ApiClient.ListUsersAsync(includeFormerEmployees: false, TestContext.Current.CancellationToken);

        users.Should().HaveCount(2);
        users[0].EmpId.Should().Be("TST");
        users[0].Email.Should().Be("test.user@example.com");
        users[1].EmpId.Should().Be("AEX");

        var post = WireMock.LogEntries
            .Single(e => e.RequestMessage!.AbsolutePath == "/api/Employees/GetByIds");
        using var doc = JsonDocument.Parse(post.RequestMessage!.Body!);
        doc.RootElement.EnumerateArray().Select(e => e.GetString()).Should().Equal("TST", "AEX");
    }

    [Fact]
    public async Task ListUsers_WithAllRequestsFormerEmployees()
    {
        WireMock.Given(Request.Create()
                .WithPath("/api/Employees/DropDown")
                .WithParam("includeExEmployees", "true")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""[{ "Text": "Former Example (FEX)", "Value": "FEX" }]"""));

        WireMock.Given(Request.Create()
                .WithPath("/api/Employees/GetByIds")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""[{ "EmpID": "FEX", "Name": "Former Example", "Email": "former.example@example.com" }]"""));

        var users = await ApiClient.ListUsersAsync(includeFormerEmployees: true, TestContext.Current.CancellationToken);

        users.Should().ContainSingle(u => u.EmpId == "FEX");
    }

    [Fact]
    public async Task GetUser_ReturnsFocusedEmployeeDetails()
    {
        WireMock.Given(Request.Create()
                .WithPath("/api/employees/TST")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                  "EmpID": "TST",
                  "FirstName": "Test",
                  "Surname": "User",
                  "Email": "test.user@example.com",
                  "Position": "Consultant",
                  "CategoryID": "DEV",
                  "DateEnd": null,
                  "IsEnabled": true,
                  "TimezoneId": "Australia/Brisbane",
                  "StartTime": "08:30:00",
                  "EndTime": "17:00:00",
                  "LunchBreakStart": "12:00:00",
                  "LunchBreakEnd": "13:00:00",
                  "TimeLessMinutes": 30,
                  "SiteID": "BNE",
                  "AccountNo": "ignored"
                }
                """));

        var user = await ApiClient.GetUserAsync("TST", TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        user!.EmpId.Should().Be("TST");
        user.Name.Should().Be("Test User");
        user.Email.Should().Be("test.user@example.com");
        user.CategoryId.Should().Be("DEV");
        user.SiteId.Should().Be("BNE");
        user.TimeLessMinutes.Should().Be(30);
    }

    [Fact]
    public async Task GetUser_HydratesMissingEmailFromEmployeeIdsEndpoint()
    {
        WireMock.Given(Request.Create()
                .WithPath("/api/employees/TST")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""
                {
                  "EmpID": "TST",
                  "FirstName": "Test",
                  "Surname": "User"
                }
                """));

        WireMock.Given(Request.Create()
                .WithPath("/api/Employees/GetByIds")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("""[{ "EmpID": "TST", "Name": "Test User", "Email": "test.user@example.com" }]"""));

        var user = await ApiClient.GetUserAsync("TST", TestContext.Current.CancellationToken);

        user.Should().NotBeNull();
        user!.Email.Should().Be("test.user@example.com");

        var post = WireMock.LogEntries
            .Single(e => e.RequestMessage!.AbsolutePath == "/api/Employees/GetByIds");
        using var doc = JsonDocument.Parse(post.RequestMessage!.Body!);
        doc.RootElement.EnumerateArray().Select(e => e.GetString()).Should().Equal("TST");
    }
}
