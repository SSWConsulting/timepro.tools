using System.ComponentModel;
using SSW.TimePro.Cli.Infrastructure.Config;
using SSW.TimePro.Cli.Infrastructure.Output;
using Spectre.Console.Cli;

namespace SSW.TimePro.Cli.Features.Skills;

[Description("Generate agent skill files for TimePro")]
public class CreateCommand : Command<CreateCommand.Settings>
{
    private readonly IConfigService _config;

    public class Settings : CommandSettings
    {
        [CommandArgument(0, "<TARGET>")]
        [Description("Target directory (e.g., .agents, .claude)")]
        public string Target { get; set; } = string.Empty;

        [CommandOption("--global")]
        [Description("Write to global config instead of local project")]
        public bool Global { get; set; }

        [CommandOption("--accounting")]
        [Description("Also write the accountant CLI skill (timepro-accounting-cli) alongside the timesheets skill. Opt-in; does not touch any existing HTTP-based timepro-accounting skill.")]
        public bool Accounting { get; set; }
    }

    public CreateCommand(IConfigService config) => _config = config;

    protected override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        var tenant = _config.LoadActiveTenantConfig();
        var global = _config.LoadGlobalConfig();
        var mappings = _config.LoadRepoMappings();

        // --global only swaps the base dir; the skill layout is always skills/<name>/SKILL.md.
        var baseDir = settings.Global
            ? ConfigPaths.Root
            : Path.Combine(Environment.CurrentDirectory, settings.Target);

        // Detect repo mapping for current directory (with worktree support)
        var repoMapping = RepoDetector.Detect(Environment.CurrentDirectory, mappings);

        // Detect git remote for GH integration
        var remoteUrl = RepoDetector.GetRemoteUrl(Environment.CurrentDirectory);
        string? ghRepoSlug = null;
        if (remoteUrl is not null && remoteUrl.Contains("github.com/"))
        {
            // Extract org/repo from github.com/org/repo
            var ghPath = remoteUrl[(remoteUrl.IndexOf("github.com/") + "github.com/".Length)..];
            ghRepoSlug = ghPath.TrimEnd('/');
        }

        var timesheets = SkillModelBuilder.BuildTimesheets(
            tenant, global, repoMapping, ghRepoSlug);
        WriteSkill(baseDir, timesheets);

        if (settings.Accounting)
        {
            var accounting = SkillModelBuilder.BuildAccounting(tenant);
            WriteSkill(baseDir, accounting);
        }

        return 0;
    }

    private static void WriteSkill(string baseDir, SkillContentModel model)
    {
        var relativePath = SkillRenderer.RelativePath(model.Name);
        var outputFile = Path.Combine(baseDir, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        File.WriteAllText(outputFile, SkillRenderer.Render(model));
        OutputHelper.WriteSuccess($"Skill file written to {outputFile}");
    }
}
