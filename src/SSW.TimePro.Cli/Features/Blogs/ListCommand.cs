using System.ComponentModel;
using SSW.TimePro.Cli.Infrastructure.ApiClient;
using SSW.TimePro.Cli.Infrastructure.Output;
using Spectre.Console;
using Spectre.Console.Cli;

namespace SSW.TimePro.Cli.Features.Blogs;

[Description("List latest blog posts from SSW employees")]
public class ListCommand : AsyncCommand<ListCommand.Settings>
{
    private readonly ITimeProApiClient _api;

    public class Settings : CommandSettings
    {
        [CommandOption("--limit <N>")]
        [Description("Number of posts to show (default: 10)")]
        public int Limit { get; set; } = 10;

        [CommandOption("--all")]
        [Description("Include blog posts from former employees")]
        public bool All { get; set; }

        [CommandOption("--json")]
        [Description("Output as JSON")]
        public bool Json { get; set; }
    }

    public ListCommand(ITimeProApiClient api) => _api = api;

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        try
        {
            var blogs = await _api.GetBlogsAsync(settings.All, CancellationToken.None);
            var limited = blogs.Take(settings.Limit).ToList();

            OutputHelper.Render(limited, settings.Json, list =>
            {
                if (list.Count == 0)
                {
                    OutputHelper.WriteInfo("No blog posts found");
                    return;
                }

                var table = new Table()
                    .AddColumn("Date")
                    .AddColumn("Author")
                    .AddColumn("Title")
                    .AddColumn("Pts")
                    .AddColumn("URL");

                foreach (var b in list)
                {
                    var date = b.BlogData?.Published?.Split('T')[0] ?? "?";
                    var isMe = b.IsMe ? $"[bold]{Markup.Escape(b.Author ?? "?")}[/]" : Markup.Escape(b.Author ?? "?");

                    table.AddRow(
                        date,
                        isMe,
                        Markup.Escape(b.BlogData?.Title ?? "?"),
                        $"{b.Points}",
                        $"[link={Markup.Escape(b.BlogData?.Url ?? "")}][dim]{TruncateUrl(b.BlogData?.Url)}[/][/]");
                }

                AnsiConsole.Write(table);

                if (blogs.Count > settings.Limit)
                    AnsiConsole.MarkupLine($"\n[dim]Showing {settings.Limit} of {blogs.Count}. Use --limit {blogs.Count} to see all.[/]");
            });

            return 0;
        }
        catch (ApiException ex)
        {
            OutputHelper.WriteError($"API error ({ex.StatusCode}): {ex.Message}");
            return 1;
        }
    }

    private static string TruncateUrl(string? url)
    {
        if (url is null) return "?";
        // Strip protocol for display
        var display = url.Replace("https://", "").Replace("http://", "");
        return display.Length > 50 ? display[..47] + "..." : display;
    }
}
