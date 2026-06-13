namespace SSW.TimePro.Cli.Shared.Models;

/// <summary>
/// Row returned by /api/ClientInvoice/rangepaged (invoice search table).
/// </summary>
public class InvoiceSearchRow
{
    public int InvoiceId { get; set; }
    public DateTime DateCreated { get; set; }
    public string? InvoiceType { get; set; }
    public string? ClientId { get; set; }
    public string? CoName { get; set; }
    /// <summary>
    /// Invoice total including GST.
    /// </summary>
    public decimal SellTotal { get; set; }
    public decimal PaidAmt { get; set; }
    public string? ExternalSyncId { get; set; }
    public int ExternalSyncStatus { get; set; }
    public DateTime? ExternalSyncTime { get; set; }
    public string? ExternalSyncType { get; set; }
    public DateTime? LastGeneratedPdfDate { get; set; }
}

/// <summary>
/// Header for a single invoice (GET /api/ClientInvoices/{id} or /api/v2/ClientInvoice/{id}).
/// Mirrors ClientInvoiceDto in the TimePRO backend.
/// </summary>
public class InvoiceHeader
{
    public int InvoiceId { get; set; }
    public string? InvoiceWithCnId { get; set; }
    public string? CategoryId { get; set; }
    public string? CurrencyId { get; set; }
    public double? ExchangeRate { get; set; }
    public string? InvoiceType { get; set; }
    public int Batch { get; set; }
    public string? ClientId { get; set; }
    public string? ProjectId { get; set; }
    public string? ClientRef { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    /// <summary>
    /// Invoice subtotal before GST.
    /// </summary>
    public decimal? SubTotal { get; set; }
    /// <summary>
    /// Invoice total including GST. Usually equals <see cref="SubTotal"/> plus <see cref="SalesTaxAmt"/>.
    /// </summary>
    public decimal? SellTotal { get; set; }
    /// <summary>
    /// GST rate as returned by the API. Calculation code should normalize because API payloads may use either 0.1 or 10 for 10%.
    /// </summary>
    public double? SalesTaxPct { get; set; }
    /// <summary>
    /// GST component of the invoice total.
    /// </summary>
    public decimal? SalesTaxAmt { get; set; }
    public decimal? CostTotal { get; set; }
    public decimal? Margin { get; set; }
    public double? MarginPct { get; set; }
    public decimal? SumOfWrittenOff { get; set; }
    public decimal? TotalMargin { get; set; }
    public double? TotalMarginPct { get; set; }
    public decimal? PaidAmt { get; set; }
    public decimal? OSAmt { get; set; }
    public string? ExportId { get; set; }
    public DateTime? DateCreated { get; set; }
    public DateTime? DateUpdated { get; set; }
    public string? EmpUpdated { get; set; }
    public string? EmpUpdatedAccountName { get; set; }
    public string? Note { get; set; }
    public string? Month { get; set; }
    public short? CollectionDays { get; set; }
    public string? NoteInternal { get; set; }
    public DateTime? DatePromisedToPay { get; set; }
    public bool IsRecurring { get; set; }
    public bool IsLocked { get; set; }
    public string? ExternalSyncType { get; set; }
    public string? ExternalSyncId { get; set; }
    public int ExternalSyncStatus { get; set; }
    public int? OutstandingDays { get; set; }
    public int PaymentTerms { get; set; }
    public bool IsCreditNote { get; set; }
    public DateTime? CreditNoteDate { get; set; }
    /// <summary>
    /// Ledger-backed remaining prepaid credit, excluding GST. Populated by the client invoice table endpoint for prepaid invoices.
    /// </summary>
    public decimal? RemainingPrepaidCredit { get; set; }
}

/// <summary>
/// Invoice line item — row from /api/ClientInvoiceProduct/invoiceID/{id}
/// or /api/v2/ClientInvoice/{id}/products.
/// </summary>
public class InvoiceLine
{
    public int InvoiceProdId { get; set; }
    public int InvoiceId { get; set; }
    public string? SkuId { get; set; }
    public string? SkuName { get; set; }
    public string? ProdName { get; set; }
    public string? CategoryId { get; set; }
    public string? AccountId { get; set; }
    public string? EmpId { get; set; }
    public double? Qty { get; set; }
    /// <summary>
    /// Unit sell price before GST.
    /// </summary>
    public decimal? SellAmt { get; set; }
    /// <summary>
    /// Line sell total before GST, usually <see cref="Qty"/> multiplied by <see cref="SellAmt"/>.
    /// </summary>
    public decimal? SellTotal { get; set; }
    public decimal? CostAmt { get; set; }
    public decimal? CostTotal { get; set; }
    public decimal? Margin { get; set; }
    public double? MarginPct { get; set; }
    public decimal? RrpAmt { get; set; }
    public decimal? RrpTotal { get; set; }
    /// <summary>
    /// GST component for this line when returned by the API.
    /// </summary>
    public decimal? SalesTaxAmt { get; set; }
    /// <summary>
    /// GST rate for this line. Calculation code should normalize because API payloads may use either 0.1 or 10 for 10%.
    /// </summary>
    public double? SalesTaxPct { get; set; }
    public decimal? DiscountPct { get; set; }
    public string? Note { get; set; }
    public string? NoteInternal { get; set; }
    public DateTime? DateStart { get; set; }
    public DateTime? DateEnd { get; set; }
    public string? Type { get; set; }
}

/// <summary>
/// Timesheet billed (or written-off) on an invoice.
/// Returned by /api/v2/Timesheets/WithNames/{Allocated|WriteOff|Unallocated}.
/// </summary>
public class InvoiceTimesheet
{
    public int TimeId { get; set; }
    public string? EmpId { get; set; }
    public string? EmpName { get; set; }
    public string? ClientId { get; set; }
    public string? ClientName { get; set; }
    public string? ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? CategoryId { get; set; }
    public string? BillableId { get; set; }
    public DateTime? DateCreated { get; set; }
    public string? TimeStart { get; set; }
    public string? TimeEnd { get; set; }
    public decimal? TotalTime { get; set; }
    /// <summary>
    /// Raw timesheet amount before GST. Used as a fallback when billable/sell totals are absent.
    /// </summary>
    public decimal? Amount { get; set; }
    /// <summary>
    /// Billable timesheet amount before GST.
    /// </summary>
    public decimal? BillableAmount { get; set; }
    /// <summary>
    /// Hourly sell price before GST.
    /// </summary>
    public decimal? SellPrice { get; set; }
    /// <summary>
    /// Allocated sell total before GST. Preferred for prepaid drawdown calculations when present.
    /// </summary>
    public decimal? SellTotal { get; set; }
    /// <summary>
    /// GST component for this timesheet allocation when returned by the API.
    /// </summary>
    public decimal? SalesTaxAmt { get; set; }
    /// <summary>
    /// GST rate for this timesheet allocation. Calculation code should normalize because API payloads may use either 0.1 or 10 for 10%.
    /// </summary>
    public double? SalesTaxPct { get; set; }
    public string? Notes { get; set; }
    public int? InvoiceId { get; set; }
}

/// <summary>
/// Client invoice table response from /api/ClientInvoice/GetByClientId/{clientId}.
/// </summary>
public class ClientInvoiceTable
{
    public List<InvoiceHeader> Invoices { get; set; } = [];
    public int Total { get; set; }
}

/// <summary>
/// Money split into ex-GST, GST, and inc-GST components.
/// </summary>
public class TaxBreakdown
{
    /// <summary>
    /// Amount before GST.
    /// </summary>
    public decimal ExGst { get; set; }
    /// <summary>
    /// GST component.
    /// </summary>
    public decimal Gst { get; set; }
    /// <summary>
    /// Amount including GST.
    /// </summary>
    public decimal IncGst { get; set; }
}

/// <summary>
/// Structured prepaid drawdown summary composed from existing invoice, timesheet,
/// credit note, and client invoice table endpoints.
/// </summary>
public class PrepaidStatusSummary
{
    public int InvoiceId { get; set; }
    public string? ClientId { get; set; }
    public string? InvoiceType { get; set; }
    public string? CurrencyId { get; set; }
    public double? ExchangeRate { get; set; }
    /// <summary>
    /// GST rate from the invoice header. Calculation code normalizes 0.1 and 10 as 10%.
    /// </summary>
    public double? SalesTaxPct { get; set; }
    /// <summary>
    /// Original prepaid invoice total split into ex-GST, GST, and inc-GST components.
    /// </summary>
    public TaxBreakdown Original { get; set; } = new();
    /// <summary>
    /// Prepaid drawdown from allocated BPP timesheets, split into ex-GST, GST, and inc-GST components.
    /// </summary>
    public TaxBreakdown DrawnDown { get; set; } = new();
    /// <summary>
    /// Crediting credit notes applied to the prepaid invoice, split into ex-GST, GST, and inc-GST components.
    /// </summary>
    public TaxBreakdown Credited { get; set; } = new();
    /// <summary>
    /// Remaining prepaid balance split into ex-GST, GST, and inc-GST components.
    /// </summary>
    public TaxBreakdown Remaining { get; set; } = new();
    public int DrawdownTimesheetCount { get; set; }
    public int CreditingCreditNoteCount { get; set; }
    public decimal ReconciliationDeltaExGst { get; set; }
    public decimal ReconciliationDeltaIncGst { get; set; }
    public string RemainingExGstSource { get; set; } = "api/ClientInvoice/GetByClientId/{clientId}.remainingPrepaidCredit";
}
