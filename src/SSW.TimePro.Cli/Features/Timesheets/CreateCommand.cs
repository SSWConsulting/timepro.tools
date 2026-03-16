using System.ComponentModel;
using System.Globalization;
using SSW.TimePro.Cli.Infrastructure.ApiClient;
using SSW.TimePro.Cli.Infrastructure.Config;
using SSW.TimePro.Cli.Infrastructure.Output;
using SSW.TimePro.Cli.Shared.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SSW.TimePro.Cli.Features.Timesheets;

[Description("Create a new timesheet entry")]
public class CreateCommand : AsyncCommand<CreateCommand.Settings>
{
    private readonly ITimeProApiClient _api;
    private readonly IConfigService _config;

    public class Settings : CommandSettings
    {
        [CommandOption("--client <CLIENT>")]
        [Description("Client ID")]
        public string ClientId { get; set; } = string.Empty;

        [CommandOption("--project <PROJECT>")]
        [Description("Project ID")]
        public string ProjectId { get; set; } = string.Empty;

        [CommandOption("--date <DATE>")]
        [Description("Date (yyyy-MM-dd). Defaults to today")]
        public string? Date { get; set; }

        [CommandOption("--start <TIME>")]
        [Description("Start time (HH:mm). Defaults to 09:00")]
        public string? Start { get; set; }

        [CommandOption("--end <TIME>")]
        [Description("End time (HH:mm). Defaults to 17:00")]
        public string? End { get; set; }

        [CommandOption("--description <DESC>")]
        [Description("Notes/description")]
        public string? Description { get; set; }

        [CommandOption("--location <LOC>")]
        [Description("Location (e.g., Office, Home, Client)")]
        public string? Location { get; set; }

        [CommandOption("--category <CAT>")]
        [Description("Category ID")]
        public string? Category { get; set; }

        [CommandOption("--billable <TYPE>")]
        [Description("Billable type: B (billable), BPP (prepaid), W (write-off)")]
        public string? Billable { get; set; }

        [CommandOption("--less <MINUTES>")]
        [Description("Break/less time in minutes")]
        public int? Less { get; set; }

        [CommandOption("--yes")]
        [Description("Skip confirmation prompt")]
        public bool Yes { get; set; }

        [CommandOption("--json")]
        [Description("Output as JSON")]
        public bool Json { get; set; }
    }

    public CreateCommand(ITimeProApiClient api, IConfigService config)
    {
        _api = api;
        _config = config;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        if (string.IsNullOrEmpty(settings.ClientId) || string.IsNullOrEmpty(settings.ProjectId))
        {
            OutputHelper.WriteError("--client and --project are required");
            return 1;
        }

        var tenant = _config.LoadActiveTenantConfig();
        if (tenant?.EmployeeId is null)
        {
            OutputHelper.WriteError("Not logged in. Run 'tp login --tenant <id>' first.");
            return 1;
        }

        var date = settings.Date is not null
            ? DateOnly.ParseExact(settings.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture)
            : DateOnly.FromDateTime(DateTime.Today);

        var startTime = settings.Start ?? "09:00";
        var endTime = settings.End ?? "17:00";
        var lessMins = settings.Less ?? 0;

        // Resolve location from WFH defaults if not specified
        var location = settings.Location;
        if (string.IsNullOrEmpty(location))
        {
            var global = _config.LoadGlobalConfig();
            var dayName = date.DayOfWeek.ToString();
            location = global.WfhDays.Contains(dayName, StringComparer.OrdinalIgnoreCase)
                ? "Home"
                : global.DefaultLocation;
        }

        try
        {
            // Check rate before creating
            var rate = await _api.GetClientRateAsync(
                tenant.EmployeeId, settings.ClientId, date, CancellationToken.None);

            if (rate is not null && !string.IsNullOrEmpty(rate.ExpiryDate))
            {
                var expiry = DateTime.Parse(rate.ExpiryDate);
                if (expiry < DateTime.Today)
                {
                    OutputHelper.WriteWarning(
                        $"Rate for client '{settings.ClientId}' expired on {rate.ExpiryDate}. Contact admin to renew.");
                    if (!settings.Yes)
                    {
                        if (!AnsiConsole.Confirm("Continue anyway?", false))
                            return 1;
                    }
                }
            }

            var request = new TimesheetRequest
            {
                EmpId = tenant.EmployeeId,
                ClientId = settings.ClientId,
                ProjectId = settings.ProjectId,
                DateCreated = date.ToString("yyyy-MM-dd"),
                TimeStart = $"{date:yyyy-MM-dd}T{startTime}:00",
                TimeEnd = $"{date:yyyy-MM-dd}T{endTime}:00",
                TimeLess = lessMins > 0 ? lessMins / 60m : null,
                Note = settings.Description,
                LocationId = location,
                CategoryId = settings.Category,
                BillableId = settings.Billable ?? "B",
                SellPrice = rate?.Rate,
            };

            // Show preview
            if (!settings.Json)
            {
                AnsiConsole.MarkupLine("[bold]Creating timesheet:[/]");
                AnsiConsole.MarkupLine($"  Date:     {date:yyyy-MM-dd} ({date:dddd})");
                AnsiConsole.MarkupLine($"  Client:   {Markup.Escape(settings.ClientId)}");
                AnsiConsole.MarkupLine($"  Project:  {Markup.Escape(settings.ProjectId)}");
                AnsiConsole.MarkupLine($"  Time:     {startTime} - {endTime}");
                AnsiConsole.MarkupLine($"  Location: {Markup.Escape(location ?? "?")}");
                AnsiConsole.MarkupLine($"  Billable: {request.BillableId}");
                if (rate?.Rate is not null)
                    AnsiConsole.MarkupLine($"  Rate:     ${rate.Rate:F2}");
                if (!string.IsNullOrEmpty(settings.Description))
                    AnsiConsole.MarkupLine($"  Notes:    {Markup.Escape(settings.Description)}");
                AnsiConsole.WriteLine();
            }

            if (!settings.Yes && !settings.Json)
            {
                if (!AnsiConsole.Confirm("Create this timesheet?"))
                    return 1;
            }

            var response = await _api.CreateTimesheetAsync(request, CancellationToken.None);

            if (settings.Json)
            {
                OutputHelper.WriteJson(response ?? new TimesheetResponse { Success = true });
            }
            else if (response is null || response.Success)
            {
                // API returns empty body on success
                OutputHelper.WriteSuccess($"Timesheet created{(response?.TimesheetId is not null ? $" (ID: {response.TimesheetId})" : "")}");
            }
            else
            {
                OutputHelper.WriteError(response.Message ?? "Failed to create timesheet");
                return 1;
            }

            return 0;
        }
        catch (ApiException ex)
        {
            OutputHelper.WriteError($"API error ({ex.StatusCode}): {ex.Message}");
            return 1;
        }
    }
}
