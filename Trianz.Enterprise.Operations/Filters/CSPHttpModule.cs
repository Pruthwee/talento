using System;
using System.Web;

namespace Trianz.Enterprise.Operations.Filters
{
    /// <summary>
    /// HTTP module that adds Content-Security-Policy and other security headers
    /// to every HTTP response. This replaces IIS-specific configuration with
    /// a portable HTTP module that works on any hosting platform.
    /// </summary>
    public class CSPHttpModule : IHttpModule
    {
        public void Init(HttpApplication context)
        {
            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            var app = (HttpApplication)sender;
            var response = app.Response;

            // Add CSP header if not already present
            if (string.IsNullOrEmpty(response.Headers["Content-Security-Policy"]))
            {
                response.Headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "script-src 'self' 'unsafe-inline' 'unsafe-eval' https://ajax.googleapis.com https://www.googletagmanager.com; " +
                    "style-src 'self' 'unsafe-inline' https://ajax.googleapis.com; " +
                    "img-src 'self' data: https:; " +
                    "font-src 'self' data:; " +
                    "connect-src 'self' https:; " +
                    "object-src 'none'; " +
                    "frame-src 'self' https:; " +
                    "base-uri 'self'; " +
                    "form-action 'self';";
            }

            // Ensure other security headers are present
            if (string.IsNullOrEmpty(response.Headers["X-Frame-Options"]))
            {
                response.Headers["X-Frame-Options"] = "DENY";
            }
            if (string.IsNullOrEmpty(response.Headers["X-XSS-Protection"]))
            {
                response.Headers["X-XSS-Protection"] = "1; mode=block";
            }
            if (string.IsNullOrEmpty(response.Headers["X-Content-Type-Options"]))
            {
                response.Headers["X-Content-Type-Options"] = "nosniff";
            }
            if (string.IsNullOrEmpty(response.Headers["Strict-Transport-Security"]))
            {
                response.Headers["Strict-Transport-Security"] = "max-age=31536000; includeSubdomains=always";
            }
        }

        public void Dispose()
        {
            // No resources to dispose
        }
    }
}
