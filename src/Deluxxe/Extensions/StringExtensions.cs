namespace Deluxxe.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// this function takes strings with w/e formatting and converts it to a form that works well with file paths, hashing, etc
    /// </summary>
    /// <param name="value">the string to be sanitized</param>
    /// <returns>a hyphenated and pretty string</returns>
    public static string Sanitize(this string value)
    {
        return value
            .Replace(' ', '-')
            .Replace('/', '-')
            .Replace('\\', '-')
            .ToLowerInvariant()
            .Trim();
    }
}