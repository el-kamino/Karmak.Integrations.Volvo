using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;

namespace Karmak.Integrations.Volvo.React.Utils
{
    public static class StringExtensions
    {

        // WAF Sanitize: Path traversal: "..", "../", "..\", encoded variants (%2e%2e), repeated dots as a whole field
        private static readonly Regex PathTraversalPattern = new Regex(
            @"^\.{2,}$|\.\.[\\/]|%2e%2e|%2E%2E",
            RegexOptions.Compiled);

        // WAF Sanitize: SQLi-ish tokens: UNION SELECT, OR 1=1, --, ;--, /*, xp_
        private static readonly Regex SqlInjectionPattern = new Regex(
            @"(\bunion\b.*\bselect\b)|(drop\s+table\b)|(\bor\b\s+\d+\s*=\s*\d+)|(--)|(;--)|(/\*)|(\bxp_cmdshell\b)",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // WAF Sanitize: XSS-ish tokens: <script>, javascript:, onerror=, <img
        private static readonly Regex XssPattern = new Regex(
            @"<\s*script|javascript:|on\w+\s*=|<\s*img|<\s*iframe",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        // WAF Sanitize: Null bytes / control characters
        private static readonly Regex ControlCharPattern = new Regex(
            @"[\x00-\x08\x0B\x0C\x0E-\x1F]",
            RegexOptions.Compiled);


        public static string MaxLength(this string input, int maxLength)
        {
            return input == null
                ? input
                : input.Substring(0, Math.Min(input.Length, maxLength));
        }

        public static string RemoveSpecialCharacters(this string input)
        {
            return string.IsNullOrEmpty(input) ? input : Regex.Replace(input, @"[^0-9a-zA-Z:]+", "");
        }

        public static bool EqualsIgnoreCase(this string input, string other)
        {
            return string.Equals(input, other, StringComparison.OrdinalIgnoreCase);
        }

        public static string DefaultIfNullOrEmpty(this string input, string defaultValue)
        {
            return string.IsNullOrEmpty(input) ? defaultValue : input;
        }

        public static string WafSanitize(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return input;

            var trimmed = input.Trim();

            // Placeholder-only values like "..", ".", "-", "N/A" collapse to empty string
            if (PathTraversalPattern.IsMatch(trimmed) ||
                Regex.IsMatch(trimmed, @"^[\-\.\s]*$"))
            {
                return "";
            }

            // Strip control chars outright
            trimmed = ControlCharPattern.Replace(trimmed, "");

            if (SqlInjectionPattern.IsMatch(trimmed) || XssPattern.IsMatch(trimmed))
            {
                // Strip the offending characters
                trimmed = Regex.Replace(trimmed, @"[<>;]", "");
            }

            return trimmed;
        }
    }
}