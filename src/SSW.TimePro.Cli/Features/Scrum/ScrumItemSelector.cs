namespace SSW.TimePro.Cli.Features.Scrum;

/// <summary>
/// Pure, side-effect-free logic for selecting which GitHub PRs and issues
/// belong in each scrum bucket (Yesterday / Today / Blockers). Mirrors
/// AutoScrum's three-bucket selection algorithm (see docs/plans/autoscrum-logic.md)
/// adapted to GitHub's state model instead of Azure DevOps StateChangeDate.
///
/// Deliberately free of IO — all inputs are plain data. Wire up in
/// <see cref="ScrumDataGatherer"/> which handles API/gh calls.
/// </summary>
public static class ScrumItemSelector
{
    // ── Input DTOs ────────────────────────────────────────────────────────

    /// <summary>A GitHub pull request as seen by the selector.</summary>
    public sealed record GitHubPr(
        int Number,
        string Title,
        string Url,
        string State,          // "OPEN", "CLOSED", "MERGED"
        bool IsDraft,
        DateTimeOffset? MergedAt,
        DateTimeOffset? UpdatedAt,
        IReadOnlyList<string> Labels);

    /// <summary>A GitHub issue as seen by the selector.</summary>
    public sealed record GitHubIssue(
        int Number,
        string Title,
        string Url,
        string State,          // "OPEN", "CLOSED"
        DateTimeOffset? ClosedAt,
        DateTimeOffset? UpdatedAt,
        IReadOnlyList<string> Labels);

    // ── Output DTOs ───────────────────────────────────────────────────────

    public sealed record Selection(
        IReadOnlyList<ScrumItem> Yesterday,
        IReadOnlyList<ScrumItem> Today,
        IReadOnlyList<ScrumItem> Blockers);

    // ── Public API ────────────────────────────────────────────────────────

    /// <summary>
    /// Classify <paramref name="prs"/> and <paramref name="issues"/> into the
    /// three scrum buckets using AutoScrum-inspired heuristics.
    /// </summary>
    /// <param name="today">The reference date for "today" (midnight, local).</param>
    /// <param name="previousWorkDay">
    ///   The most recent working day before <paramref name="today"/>
    ///   (Friday when today is Monday; otherwise today-1). Callers must skip
    ///   weekends; public-holiday awareness is a future enhancement.
    /// </param>
    /// <param name="hadPreviousProjectDay">
    ///   True when the caller found a matching timesheet on <paramref name="previousWorkDay"/>
    ///   for the same project(s) as today. Used by the "still in-progress" yesterday rule.
    /// </param>
    /// <param name="prs">PRs to classify.</param>
    /// <param name="issues">Issues to classify.</param>
    public static Selection Select(
        DateOnly today,
        DateOnly previousWorkDay,
        bool hadPreviousProjectDay,
        IEnumerable<GitHubPr> prs,
        IEnumerable<GitHubIssue> issues)
    {
        // Work with midnight-local DateTimeOffset values for comparisons.
        var todayMidnight = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
        var prevMidnight = previousWorkDay.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);

        var yesterdayItems = new List<ScrumItem>();
        var todayItems = new List<ScrumItem>();
        var blockerItems = new List<ScrumItem>();

        // Track IDs already placed to avoid duplicates across buckets.
        var inYesterday = new HashSet<string>();
        var inToday = new HashSet<string>();
        var inBlockers = new HashSet<string>();

        // ── PRs ───────────────────────────────────────────────────────────

        foreach (var pr in prs)
        {
            var key = $"PR#{pr.Number}";
            var isBlocked = IsBlocked(pr.Labels) || pr.IsDraft;

            // Blocker check first — blocked items bubble up regardless of state.
            if (isBlocked && pr.State == "OPEN")
            {
                blockerItems.Add(MakePrItem(pr, "❌ Blocked"));
                inBlockers.Add(key);
                // Also appears in Today (consistent with AutoScrum's blocker rendering).
            }

            if (pr.State == "OPEN")
            {
                // Today: any open PR.
                if (!inToday.Contains(key))
                {
                    todayItems.Add(MakePrItem(pr, status: string.Empty));
                    inToday.Add(key);
                }

                // Yesterday: open PR that was already in-flight on the previous project day.
                // Rule mirrors AutoScrum: InProgress with StateChangeDate < todayMidnight.
                if (hadPreviousProjectDay &&
                    pr.UpdatedAt.HasValue &&
                    pr.UpdatedAt.Value.LocalDateTime < todayMidnight &&
                    !inYesterday.Contains(key))
                {
                    yesterdayItems.Add(MakePrItem(pr, status: string.Empty));
                    inYesterday.Add(key);
                }
            }
            else if (pr.State is "MERGED" or "CLOSED")
            {
                // Yesterday: merged/closed in the window [previousWorkDayMidnight, todayMidnight).
                if (pr.MergedAt.HasValue &&
                    pr.MergedAt.Value.LocalDateTime >= prevMidnight &&
                    pr.MergedAt.Value.LocalDateTime < todayMidnight &&
                    !inYesterday.Contains(key))
                {
                    yesterdayItems.Add(MakePrItem(pr, "✅ Done"));
                    inYesterday.Add(key);
                }
            }
        }

        // ── Issues ────────────────────────────────────────────────────────

        foreach (var issue in issues)
        {
            var key = $"Issue#{issue.Number}";
            var isBlocked = IsBlocked(issue.Labels);

            if (isBlocked && issue.State == "OPEN")
            {
                blockerItems.Add(MakeIssueItem(issue, "❌ Blocked"));
                inBlockers.Add(key);
            }

            if (issue.State == "OPEN")
            {
                // Today: any open issue assigned to me.
                if (!inToday.Contains(key))
                {
                    todayItems.Add(MakeIssueItem(issue, status: string.Empty));
                    inToday.Add(key);
                }

                // Yesterday: open issue that was already in-flight on the previous project day.
                if (hadPreviousProjectDay &&
                    issue.UpdatedAt.HasValue &&
                    issue.UpdatedAt.Value.LocalDateTime < todayMidnight &&
                    !inYesterday.Contains(key))
                {
                    yesterdayItems.Add(MakeIssueItem(issue, status: string.Empty));
                    inYesterday.Add(key);
                }
            }
            else if (issue.State == "CLOSED")
            {
                // Yesterday: closed in the [previousWorkDayMidnight, todayMidnight) window.
                if (issue.ClosedAt.HasValue &&
                    issue.ClosedAt.Value.LocalDateTime >= prevMidnight &&
                    issue.ClosedAt.Value.LocalDateTime < todayMidnight &&
                    !inYesterday.Contains(key))
                {
                    yesterdayItems.Add(MakeIssueItem(issue, "✅ Done"));
                    inYesterday.Add(key);
                }
            }
        }

        return new Selection(yesterdayItems, todayItems, blockerItems);
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the most-recent working day before <paramref name="today"/>,
    /// skipping Saturday and Sunday. Mirrors AutoScrum's DateService.GetPreviousWorkDay.
    /// </summary>
    public static DateOnly PreviousWorkDay(DateOnly today)
    {
        var candidate = today.AddDays(-1);
        while (candidate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
            candidate = candidate.AddDays(-1);
        return candidate;
    }

    private static bool IsBlocked(IReadOnlyList<string> labels) =>
        labels.Any(l => l.Contains("blocked", StringComparison.OrdinalIgnoreCase));

    private static ScrumItem MakePrItem(GitHubPr pr, string status) => new()
    {
        Status = status,
        Kind = "PR",
        Title = pr.Title,
        Url = pr.Url,
        Reference = $"#{pr.Number}"
    };

    private static ScrumItem MakeIssueItem(GitHubIssue issue, string status) => new()
    {
        Status = status,
        Kind = "PBI",
        Title = issue.Title,
        Url = issue.Url,
        Reference = $"#{issue.Number}"
    };
}
