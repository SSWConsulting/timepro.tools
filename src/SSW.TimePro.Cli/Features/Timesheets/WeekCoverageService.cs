using SSW.TimePro.Cli.Infrastructure.ApiClient;
using SSW.TimePro.Cli.Shared.Models;

namespace SSW.TimePro.Cli.Features.Timesheets;

/// <summary>
/// Shared orchestration for the leave-aware weekly timesheet check: resolves the
/// Mon–Fri window, fetches the week's timesheets + approved leave (UPCOMING + PAST),
/// and runs <see cref="CheckEvaluator"/> per day.
///
/// Used by BOTH <see cref="CheckCommand"/> (CLI) and the MCP <c>CheckWeek</c> tool so
/// the fetch/merge logic lives in exactly one place — each caller only shapes output.
/// </summary>
public static class WeekCoverageService
{
    private const int LeavePageSize = 200;

    /// <summary>The computed coverage for one work week.</summary>
    public sealed record WeekCoverage(
        string EmpId,
        DateOnly Monday,
        DateOnly Friday,
        int Errors,
        int Warnings,
        int Infos,
        bool AllCovered,
        IReadOnlyList<CheckEvaluator.DayCheck> Days);

    /// <summary>
    /// Evaluate the Mon–Fri coverage for <paramref name="empId"/> at the given week
    /// offset (0 = this week, -1 = last week).
    /// </summary>
    public static async Task<WeekCoverage> EvaluateWeekAsync(
        ITimeProApiClient api, string empId, int weekOffset, CancellationToken ct = default)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var monday = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday + (weekOffset * 7));
        if (today.DayOfWeek == DayOfWeek.Sunday)
            monday = monday.AddDays(-7);
        var friday = monday.AddDays(4);

        // Fetch leave ONCE — the checked week may be entirely past (e.g. --week -1) or a
        // Mon–Thu leave may already be "past" by Friday, so merge UPCOMING + PAST.
        var approvedLeave = await LoadApprovedLeaveAsync(api, empId, ct);

        var days = new List<CheckEvaluator.DayCheck>();
        int errors = 0, warnings = 0, infos = 0;

        // Per-day loop is intentional. The server's week-range aggregates do NOT carry the
        // per-entry fields CheckEvaluator depends on:
        //   • /api/Timesheets/Summary (TimesheetFullCalendarDto) → only {id,title,allDay,start,
        //     end,color,textColor}: no Notes/HasNotes, no IsSuggested, no TotalTime.
        //   • /api/TimesheetsApi (TimesheetModel) → has rows but NO IsSuggested flag.
        // Swapping to either would regress the missing-description / suggested-vs-real / overlap
        // checks, so we keep GetTimesheetListViewModel per day (the only shape with all fields).
        for (var d = monday; d <= friday; d = d.AddDays(1))
        {
            var timesheets = await api.GetTimesheetsAsync(empId, d, ct);
            var real = timesheets.Where(t => !t.IsSuggested).ToList();
            var suggestedCount = timesheets.Count(t => t.IsSuggested);

            var check = CheckEvaluator.EvaluateDay(d, real, suggestedCount, approvedLeave);
            days.Add(check);

            errors += check.Issues.Count(i => i.Severity == "error");
            warnings += check.Issues.Count(i => i.Severity == "warning");
            infos += check.Issues.Count(i => i.Severity == "info");
        }

        return new WeekCoverage(empId, monday, friday, errors, warnings, infos,
            days.All(c => c.Covered), days);
    }

    private static async Task<List<CheckEvaluator.LeaveDay>> LoadApprovedLeaveAsync(
        ITimeProApiClient api, string empId, CancellationToken ct)
    {
        var entries = new List<LeaveEntry>();
        foreach (var filter in new[] { "UPCOMING", "PAST" })
        {
            var response = await api.GetLeaveAsync(filter, 1, LeavePageSize, empId, ct);
            var items = response?.Leaves?.Items;
            if (items is not null)
                entries.AddRange(items);
        }
        return CheckEvaluator.ToLeaveDays(entries);
    }
}
