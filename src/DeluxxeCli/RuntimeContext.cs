namespace DeluxxeCli;

public class RuntimeContext
{
    public required DirectoryInfo outputDir { get; init; }

    public string? uniqueEventName { get; init; }

    public string? date { get; init; }

    public string? season
    {
        get
        {
            if (uniqueEventName is null)
            {
                return null;
            }

            var dateMatch = System.Text.RegularExpressions.Regex.Match(uniqueEventName, @"^(\d{4})-");
            return dateMatch.Success ? dateMatch.Groups[1].Value : null;
        }
    }

    public string? eventName
    {
        get
        {
            if (uniqueEventName is null)
            {
                return null;
            }

            var eventMatch = System.Text.RegularExpressions.Regex.Match(uniqueEventName, @"^\d{4}-\d{2}-\d{2}-([a-z-]+)$");
            return eventMatch.Success ? eventMatch.Groups[1].Value : null;
        }
    }
}