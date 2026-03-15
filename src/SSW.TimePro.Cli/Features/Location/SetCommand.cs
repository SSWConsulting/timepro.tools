using System.ComponentModel;
using SSW.TimePro.Cli.Infrastructure.Config;
using SSW.TimePro.Cli.Infrastructure.Output;
using Spectre.Console.Cli;

namespace SSW.TimePro.Cli.Features.Location;

[Description("Set WFH day defaults")]
public class SetCommand : Command<SetCommand.Settings>
{
    private readonly IConfigService _config;

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<LOCATION>")]
        [Description("Location name (e.g., Home, Office)")]
        public string Location { get; set; } = string.Empty;

        [CommandOption("--day <DAYS>")]
        [Description("Comma-separated days (e.g., Mon,Tue,Wed)")]
        public string? Days { get; set; }
    }

    public SetCommand(IConfigService config) => _config = config;

    public override int Execute(CommandContext context, Settings settings)
    {
        var global = _config.LoadGlobalConfig();

        if (settings.Days is not null)
        {
            // Setting WFH for specific days
            var dayMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["mon"] = "Monday", ["tue"] = "Tuesday", ["wed"] = "Wednesday",
                ["thu"] = "Thursday", ["fri"] = "Friday",
                ["monday"] = "Monday", ["tuesday"] = "Tuesday", ["wednesday"] = "Wednesday",
                ["thursday"] = "Thursday", ["friday"] = "Friday"
            };

            var days = new List<string>();
            foreach (var raw in settings.Days.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (dayMap.TryGetValue(raw, out var full))
                    days.Add(full);
                else
                {
                    OutputHelper.WriteError($"Unknown day: '{raw}'. Use Mon,Tue,Wed,Thu,Fri.");
                    return 1;
                }
            }

            if (settings.Location.Equals("Home", StringComparison.OrdinalIgnoreCase))
            {
                global.WfhDays = days;
            }
            else if (settings.Location.Equals("Office", StringComparison.OrdinalIgnoreCase))
            {
                // Setting office days = remove those from WFH
                global.WfhDays = global.WfhDays
                    .Where(d => !days.Contains(d, StringComparer.OrdinalIgnoreCase))
                    .ToList();
            }
        }
        else
        {
            // Setting default location
            global.DefaultLocation = settings.Location;
        }

        _config.SaveGlobalConfig(global);
        OutputHelper.WriteSuccess($"Location defaults updated");

        if (global.WfhDays.Count > 0)
            OutputHelper.WriteInfo($"WFH days: {string.Join(", ", global.WfhDays)}");

        OutputHelper.WriteInfo($"Default location: {global.DefaultLocation}");

        return 0;
    }
}
