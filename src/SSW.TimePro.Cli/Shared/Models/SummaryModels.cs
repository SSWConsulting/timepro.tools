namespace SSW.TimePro.Cli.Shared.Models;

public class ProjectSummaryItem
{
    public string? ProjectName { get; set; }
    public string? ClientID { get; set; }
    public string? ProjectID { get; set; }
    public bool IsBillable { get; set; }
    public decimal Sum { get; set; }
    public string? StartDate { get; set; }
    public string? EndDate { get; set; }
}

public class IterationItem
{
    public int IterationId { get; set; }
    public string? IterationName { get; set; }
    public string? LastUsedDate { get; set; }
}
