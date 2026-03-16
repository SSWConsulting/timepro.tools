namespace SSW.TimePro.Cli.Shared.Models;

public class BlogEntry
{
    public string? GravatarImageUrl { get; set; }
    public BlogData? BlogData { get; set; }
    public bool IsMe { get; set; }
    public int Points { get; set; }
    public string? Author { get; set; }
}

public class BlogData
{
    public string? Title { get; set; }
    public string? Url { get; set; }
    public string? Published { get; set; }
}
