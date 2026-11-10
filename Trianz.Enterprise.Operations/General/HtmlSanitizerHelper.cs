using Ganss.Xss;
using System.Web.Mvc;

namespace Trianz.Enterprise.Operations.General
{
    /// <summary>
    /// Provides HTML sanitization to replace @Html.Raw() usage in Razor views.
    /// Uses the Ganss.Xss HtmlSanitizer library to clean HTML content,
    /// preventing XSS attacks while preserving safe markup.
    /// </summary>
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        /// <summary>
        /// Sanitizes HTML content and returns it as an HtmlString for safe rendering.
        /// Use this instead of @Html.Raw() to prevent XSS attacks.
        /// </summary>
        public static HtmlString Sanitize(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return new HtmlString("");
            }

            var sanitized = _sanitizer.Sanitize(html);
            return new HtmlString(sanitized);
        }

        /// <summary>
        /// Sanitizes HTML content and returns it as a string.
        /// </summary>
        public static string SanitizeToString(string html)
        {
            if (string.IsNullOrEmpty(html))
            {
                return "";
            }

            return _sanitizer.Sanitize(html);
        }
    }
}
