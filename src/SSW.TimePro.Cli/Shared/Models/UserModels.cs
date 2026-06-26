using System.Text.Json.Serialization;

namespace SSW.TimePro.Cli.Shared.Models;

/// <summary>
/// Response from GET /api/Employees/GetEmployeeID.
/// </summary>
public class EmployeeIdResponse
{
    public string? EmpId { get; set; }
}

/// <summary>
/// Response from GET /api/v2/users/me.
/// </summary>
public class CurrentUserResponse
{
    [JsonPropertyName("id")]
    public string? EmployeeId { get; set; }
    public string? FirstName { get; set; }
    [JsonPropertyName("surname")]
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? DefaultRate { get; set; }
}

/// <summary>
/// Response from GET /api/employees/getSettingsDetails.
/// </summary>
public class EmployeeSettings
{
    public string? EmployeeId { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public string? LunchBreakStart { get; set; }
    public string? LunchBreakEnd { get; set; }
    public int TimeLessMinutes { get; set; }
    public string? TimezoneId { get; set; }
}

public class EmployeeDropdownItem
{
    public string? Text { get; set; }
    public string? Value { get; set; }
}

public class EmployeeSummary
{
    public string? EmpId { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
}

public class EmployeeDetail
{
    public string? EmpId { get; set; }
    public string? FirstName { get; set; }
    public string? Surname { get; set; }
    public string? MiddleName { get; set; }
    public string? Position { get; set; }
    public string? CategoryId { get; set; }
    public string? Email { get; set; }
    public DateTime? DateEnd { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public bool? IsEnabled { get; set; }
    public string? TimezoneId { get; set; }
    public bool? EnableGamification { get; set; }
    public string? BlogRssFeedUrl { get; set; }
    public string? TwitterHandle { get; set; }
    public int? ViewPageId { get; set; }
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }
    public int? TimeLessMinutes { get; set; }
    public string? LunchBreakStart { get; set; }
    public string? LunchBreakEnd { get; set; }
    public string? SiteId { get; set; }

    public string? Name
    {
        get
        {
            var name = $"{FirstName} {Surname}".Trim();
            return string.IsNullOrWhiteSpace(name) ? null : name;
        }
    }
}
