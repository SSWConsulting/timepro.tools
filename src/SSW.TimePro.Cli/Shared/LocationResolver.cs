namespace SSW.TimePro.Cli.Shared;

/// <summary>
/// Maps user-friendly location names (e.g. "Office", "At Home") to valid
/// TimePro API location IDs (SSW, Client, Home, Travel, Other).
/// Falls through unrecognised values so valid IDs still work directly.
/// </summary>
public static class LocationResolver
{
    /// <summary>
    /// Known aliases → canonical TimePro LocationID.
    /// Keys are lowercase for case-insensitive lookup.
    /// </summary>
    private static readonly Dictionary<string, string> Aliases = new(StringComparer.OrdinalIgnoreCase)
    {
        // "At My Company" variants
        ["office"] = "SSW",
        ["company"] = "SSW",
        ["at my company"] = "SSW",

        // "At Home" variants
        ["home"] = "Home",
        ["at home"] = "Home",
        ["wfh"] = "Home",

        // "At Client" variants
        ["client"] = "Client",
        ["at client"] = "Client",
        ["onsite"] = "Client",

        // "Travel" variants
        ["travel"] = "Travel",
        ["travelling"] = "Travel",

        // "Other"
        ["other"] = "Other",

        // Identity mappings for the canonical IDs themselves
        ["ssw"] = "SSW",
    };

    /// <summary>
    /// Resolves a user-provided location string to a valid TimePro LocationID.
    /// Returns the canonical ID if recognised, or the original value as-is
    /// so the API can validate it.
    /// </summary>
    public static string Resolve(string location)
    {
        return Aliases.TryGetValue(location, out var canonical)
            ? canonical
            : location;
    }
}
