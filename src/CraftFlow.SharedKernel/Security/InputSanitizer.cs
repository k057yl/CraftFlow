using System.Text.RegularExpressions;

namespace CraftFlow.SharedKernel.Security
{
    public static class InputSanitizer
    {
        public static string Sanitize(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var sanitized = Regex.Replace(input, @"<[^>]*>", string.Empty);
            return sanitized.Trim();
        }
    }
}
