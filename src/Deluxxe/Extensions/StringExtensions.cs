namespace Deluxxe.Extensions;

public static class StringExtensions
{
    public static string Sanitize(this string value)
    {
        return value
            .Replace(' ', '-')
            .Replace('/', '-')
            .Replace('\\', '-')
            .ToLower()
            .Trim();
    }
}