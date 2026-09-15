using System.Text.RegularExpressions;

namespace CraftFlow.SharedKernel.Security;

public static class InputSanitizer
{
    private static readonly Regex HtmlTagsRegex = new(@"<[^>]*>", RegexOptions.Compiled);
    private static readonly Regex MultipleSpacesRegex = new(@"\s+", RegexOptions.Compiled);

    public static string Sanitize(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        var sanitized = input.Replace("\0", string.Empty);

        sanitized = HtmlTagsRegex.Replace(sanitized, string.Empty);
        sanitized = MultipleSpacesRegex.Replace(sanitized, " ");

        return sanitized.Trim();
    }
}