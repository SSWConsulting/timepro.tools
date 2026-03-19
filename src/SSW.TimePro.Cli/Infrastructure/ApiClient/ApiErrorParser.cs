using System.Text.Json;

namespace SSW.TimePro.Cli.Infrastructure.ApiClient;

/// <summary>
/// Extracts human-readable error details from TimePro API problem+json responses.
/// </summary>
public static class ApiErrorParser
{
    /// <summary>
    /// Attempts to extract validation details from a problem+json response body.
    /// Returns a combined string of all validation messages, or null if parsing fails.
    /// </summary>
    public static string? ExtractDetail(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(responseBody);
            var root = doc.RootElement;

            // Collect all string properties that look like validation messages.
            // TimePro returns flat objects like:
            //   { "title": "...", "status": 400, "detail": "...", "CategoryID": "Please specify..." }
            var messages = new List<string>();
            var skipKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                { "type", "title", "status", "detail", "traceId" };

            foreach (var prop in root.EnumerateObject())
            {
                if (skipKeys.Contains(prop.Name))
                    continue;

                if (prop.Value.ValueKind == JsonValueKind.String)
                {
                    messages.Add($"{prop.Name}: {prop.Value.GetString()}");
                }
            }

            if (messages.Count > 0)
                return string.Join("; ", messages);

            // Fallback to "detail" field
            if (root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                return detail.GetString();

            return null;
        }
        catch
        {
            // Not JSON — return raw body if short enough
            return responseBody.Length <= 200 ? responseBody : null;
        }
    }
}
