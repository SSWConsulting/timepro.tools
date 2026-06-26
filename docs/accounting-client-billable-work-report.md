# Client billable work report

This report lists clients with at least a chosen amount of billable work in a rolling date window, and includes each client's first invoice date.

Definition:

- Work threshold: `B` and `BPP` timesheets in the date window.
- Work value: `totalHours * sellPrice`, reported ex-GST.
- First invoice: earliest invoice `DateCreated` in the available invoice history up to the report end date.
- Invoice totals: invoice headers in the date window, reported ex-GST and inc-GST.
- Output: CSV by default, or JSON with `--json`.

The report is a `tp` CLI command. It uses the active tenant config and does not require a separate script or direct API credentials.

## Requirements

- .NET SDK installed.
- TimePro CLI installed as `tp`.
- A valid TimePro tenant login.

## Install or update the CLI

### macOS

```bash
curl -fsSL https://raw.githubusercontent.com/SSWConsulting/TimePro.Tools/main/scripts/install.sh | bash
```

Make sure the .NET tool directory is on your `PATH`:

```bash
export PATH="$HOME/.dotnet/tools:$PATH"
tp --help
```

### Windows

From PowerShell:

```powershell
irm https://raw.githubusercontent.com/SSWConsulting/TimePro.Tools/main/scripts/install.ps1 | iex
```

Make sure the .NET tool directory is on your `PATH`:

```powershell
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"
tp --help
```

## Login

Use the tenant name supplied by your TimePro administrator:

```bash
tp login --tenant <tenant-name>
```

If you have multiple tenant configs, set the active tenant before running the report:

```bash
tp tenant set <tenant-name>
```

Confirm the login is usable:

```bash
tp invoice list --limit 1 --json
```

## Run on macOS

```bash
tp client billable-work
```

Run the exact window and threshold explicitly:

```bash
tp client billable-work \
  --from 2025-06-26 \
  --to 2026-06-26 \
  --threshold 50000 \
  --output ./Reports/timepro-client-work.csv
```

## Run on Windows

The command defaults to the last 12 months ending today, a threshold of `$50,000`, and a CSV under `.\Reports` from the directory where you run the command.

```powershell
tp client billable-work
```

Run the exact window and threshold explicitly:

```powershell
tp client billable-work `
  --from 2025-06-26 `
  --to 2026-06-26 `
  --threshold 50000 `
  --output .\Reports\timepro-client-work.csv
```

## Output

Each run prints the generated CSV path. The repository ignores `Reports/` so generated report files are not accidentally added to source control.

For AI/tooling, emit JSON to stdout instead of writing a CSV:

```bash
tp client billable-work --from 2025-06-26 --to 2026-06-26 --threshold 50000 --json
```

JSON output is a report envelope, not a bare array. Client rows are under
`.rows[]`:

```bash
tp client billable-work --from 2025-06-26 --to 2026-06-26 --threshold 50000 --json \
  | jq '.rows | map({clientId, clientName, firstInvoiceDate, billableTimesheetValueExGst})'
```

## Troubleshooting

- `tp` not found: install the CLI and make sure the .NET tool directory is on `PATH`.
- Not logged in: run `tp login --tenant <tenant-name>`.
- Wrong tenant: run `tp tenant set <tenant-name>`.
- Slow run: the command fetches all timesheets in the window, full invoice history, and invoice headers for qualifying clients.
