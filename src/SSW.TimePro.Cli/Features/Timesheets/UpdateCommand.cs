using System.ComponentModel;
using SSW.TimePro.Cli.Infrastructure.ApiClient;
using SSW.TimePro.Cli.Infrastructure.Config;
using SSW.TimePro.Cli.Infrastructure.Output;
using SSW.TimePro.Cli.Shared.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SSW.TimePro.Cli.Features.Timesheets;

[Description("Update an existing timesheet entry")]
public class UpdateCommand : AsyncCommand<UpdateCommand.Settings>
{
    private readonly ITimeProApiClient _api;
    private readonly IConfigService _config;

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<ID>")]
        [Description("Timesheet ID to update")]
        public int TimesheetId { get; set; }

        [CommandOption("--location <LOC>")]
        [Description("New location")]
        public string? Location { get; set; }

        [CommandOption("--description <DESC>")]
        [Description("New notes/description")]
        public string? Description { get; set; }

        [CommandOption("--start <TIME>")]
        [Description("New start time (HH:mm)")]
        public string? Start { get; set; }

        [CommandOption("--end <TIME>")]
        [Description("New end time (HH:mm)")]
        public string? End { get; set; }

        [CommandOption("--client <CLIENT>")]
        [Description("New client ID")]
        public string? ClientId { get; set; }

        [CommandOption("--project <PROJECT>")]
        [Description("New project ID")]
        public string? ProjectId { get; set; }

        [CommandOption("--category <CAT>")]
        [Description("New category ID")]
        public string? Category { get; set; }

        [CommandOption("--billable <TYPE>")]
        [Description("New billable type: B, BPP, or W")]
        public string? Billable { get; set; }

        [CommandOption("--yes")]
        [Description("Skip confirmation prompt")]
        public bool Yes { get; set; }

        [CommandOption("--json")]
        [Description("Output as JSON")]
        public bool Json { get; set; }
    }

    public UpdateCommand(ITimeProApiClient api, IConfigService config)
    {
        _api = api;
        _config = config;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var tenant = _config.LoadActiveTenantConfig();
        if (tenant?.EmployeeId is null)
        {
            OutputHelper.WriteError("Not logged in. Run 'tp login --tenant <id>' first.");
            return 1;
        }

        try
        {
            // Find the existing timesheet by searching today or a range
            // We need the date to look up, so we fetch from the API
            // The API requires a date to fetch timesheets, so let's search recent days
            // For now, we'll require the user to know the timesheet ID and we'll build the request

            // Build update request with only changed fields
            var request = new TimesheetRequest
            {
                TimeId = settings.TimesheetId,
                EmpId = tenant.EmployeeId,
            };

            var changes = new List<string>();

            if (settings.Location is not null)
            {
                request.LocationId = settings.Location;
                changes.Add($"Location -> {settings.Location}");
            }
            if (settings.Description is not null)
            {
                request.Note = settings.Description;
                changes.Add($"Description -> {(settings.Description.Length > 50 ? settings.Description[..50] + "..." : settings.Description)}");
            }
            if (settings.Start is not null)
                changes.Add($"Start -> {settings.Start}");
            if (settings.End is not null)
                changes.Add($"End -> {settings.End}");
            if (settings.ClientId is not null)
            {
                request.ClientId = settings.ClientId;
                changes.Add($"Client -> {settings.ClientId}");
            }
            if (settings.ProjectId is not null)
            {
                request.ProjectId = settings.ProjectId;
                changes.Add($"Project -> {settings.ProjectId}");
            }
            if (settings.Category is not null)
            {
                request.CategoryId = settings.Category;
                changes.Add($"Category -> {settings.Category}");
            }
            if (settings.Billable is not null)
            {
                request.BillableId = settings.Billable;
                changes.Add($"Billable -> {settings.Billable}");
            }

            if (changes.Count == 0)
            {
                OutputHelper.WriteInfo("No changes specified. Use --location, --description, etc.");
                return 0;
            }

            // Show preview
            if (!settings.Json)
            {
                AnsiConsole.MarkupLine($"[bold]Updating timesheet #{settings.TimesheetId}:[/]");
                foreach (var change in changes)
                    AnsiConsole.MarkupLine($"  {Markup.Escape(change)}");
                AnsiConsole.WriteLine();
            }

            if (!settings.Yes && !settings.Json)
            {
                if (!AnsiConsole.Confirm("Apply these changes?"))
                    return 1;
            }

            var response = await _api.UpdateTimesheetAsync(request, CancellationToken.None);

            if (settings.Json)
            {
                OutputHelper.WriteJson(response);
            }
            else if (response?.Success == true)
            {
                OutputHelper.WriteSuccess($"Timesheet #{settings.TimesheetId} updated");
            }
            else
            {
                OutputHelper.WriteError(response?.Message ?? "Failed to update timesheet");
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
