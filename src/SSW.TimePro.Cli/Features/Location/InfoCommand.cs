using System.ComponentModel;
using System.Globalization;
using SSW.TimePro.Cli.Infrastructure.Config;
using SSW.TimePro.Cli.Infrastructure.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SSW.TimePro.Cli.Features.Location;

[Description("Show location defaults and WFH days")]
public class InfoCommand : Command<InfoCommand.Settings>
{
    private readonly IConfigService _config;

    public class Settings : CommandSettings
    {
        [CommandOption("--date <DATE>")]
        [Description("Check location for a specific date")]
        public string? Date { get; set; }

        [CommandOption("--json")]
        [Description("Output as JSON")]
        public bool Json { get; set; }
    }

    public InfoCommand(IConfigService config) => _config = config;

    public override int Execute(CommandContext context, Settings settings)
    {
        var global = _config.LoadGlobalConfig();

        if (settings.Date is not null)
        {
            var date = DateOnly.ParseExact(settings.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
            var dayName = date.DayOfWeek.ToString();
            var isWfh = global.WfhDays.Contains(dayName, StringComparer.OrdinalIgnoreCase);
            var location = isWfh ? "Home" : global.DefaultLocation;

            OutputHelper.Render(new { date = settings.Date, dayOfWeek = dayName, location, isWfh }, settings.Json, d =>
            {
                AnsiConsole.MarkupLine($"[bold]{date:dddd, MMM d yyyy}[/]: {Markup.Escape(location)}{(isWfh ? " [dim](WFH day)[/]" : "")}");
            });
            return 0;
        }

        var data = new
        {
            defaultLocation = global.DefaultLocation,
            wfhDays = global.WfhDays
        };

        OutputHelper.Render(data, settings.Json, d =>
        {
            AnsiConsole.MarkupLine($"[bold]Default location:[/] {Markup.Escape(global.DefaultLocation)}");
            if (global.WfhDays.Count > 0)
                AnsiConsole.MarkupLine($"[bold]WFH days:[/]         {string.Join(", ", global.WfhDays)}");
            else
                AnsiConsole.MarkupLine("[bold]WFH days:[/]         [dim]None set[/]");
        });

        return 0;
    }
}
