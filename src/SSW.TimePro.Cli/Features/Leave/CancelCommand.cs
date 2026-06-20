using System.ComponentModel;
using SSW.TimePro.Cli.Infrastructure.ApiClient;
using SSW.TimePro.Cli.Infrastructure.Output;
using SSW.TimePro.Cli.Shared.Models;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SSW.TimePro.Cli.Features.Leave;

[Description("Cancel a leave request")]
public class CancelCommand : AsyncCommand<CancelCommand.Settings>
{
    private readonly ITimeProApiClient _api;

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<ID>")]
        [Description("Leave entry ID")]
        public string LeaveId { get; set; } = string.Empty;

        [CommandOption("--reason <REASON>")]
        [Description("Cancellation reason")]
        public string? Reason { get; set; }

        [CommandOption("--yes")]
        [Description("Skip confirmation")]
        public bool Yes { get; set; }

        [CommandOption("--json")]
        [Description("Output as JSON")]
        public bool Json { get; set; }
    }

    public CancelCommand(ITimeProApiClient api) => _api = api;

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(settings.LeaveId, out var leaveId))
        {
            // The cancel route is constrained to {id:guid}; reject non-GUID values before routing 404s.
            WriteValidationError(settings.Json, "Leave ID must be a valid GUID");
            return 1;
        }

        if (string.IsNullOrWhiteSpace(settings.Reason))
        {
            WriteValidationError(settings.Json, "--reason is required: a cancellation reason is mandatory for leave");
            return 1;
        }

        var normalizedLeaveId = leaveId.ToString();
        var cancellationReason = settings.Reason.Trim();

        if (!settings.Yes && !settings.Json)
        {
            if (!AnsiConsole.Confirm($"Cancel leave request {normalizedLeaveId}?", false))
                return 1;
        }

        try
        {
            await _api.CancelLeaveAsync(
                normalizedLeaveId,
                new CancelLeaveRequest
                {
                    LeaveId = normalizedLeaveId,
                    CancellationReason = cancellationReason
                },
                CancellationToken.None);

            if (settings.Json)
                OutputHelper.WriteJson(new { success = true, leaveId = normalizedLeaveId });
            else
                OutputHelper.WriteSuccess($"Leave request cancelled");

            return 0;
        }
        catch (ApiException ex)
        {
            if (settings.Json)
                OutputHelper.WriteJsonError($"API error: {ex.Message}", ex.StatusCode);
            OutputHelper.WriteError($"API error ({ex.StatusCode}): {ex.Message}");
            return 1;
        }
    }

    private static void WriteValidationError(bool json, string message)
    {
        if (json)
            OutputHelper.WriteJsonError(message);
        OutputHelper.WriteError(message);
    }
}
