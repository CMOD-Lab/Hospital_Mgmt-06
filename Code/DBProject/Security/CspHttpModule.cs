using System;
using System.Web;

namespace DBProject.Security
{
    /// <summary>
    /// cr-dotnet-1041: Custom HttpModule that injects Content-Security-Policy (CSP) headers
    /// into every HTTP response for ASP.NET Web Forms.
    ///
    /// This module satisfies SOC2 and penetration-test compliance requirements for cloud
    /// SaaS platforms deployed on AWS.  It works in tandem with AWS WAF managed rules
    /// (e.g., AWSManagedRulesCommonRuleSet) to provide defence-in-depth against XSS
    /// attacks in multi-tenant cloud environments.
    ///
    /// The CSP policy value can be overridden at runtime via the
    /// "CSP_POLICY" environment variable so that AWS ECS / Elastic Beanstalk
    /// environment-specific policies can be injected without a code change.
    /// </summary>
    public class CspHttpModule : IHttpModule
    {
        // Default CSP policy – restrictive baseline suitable for a hospital management
        // Web Forms application.  Override via the CSP_POLICY environment variable.
        private static readonly string DefaultCspPolicy =
            "default-src 'self'; " +
            "script-src 'self' 'unsafe-inline' 'unsafe-eval'; " +
            "style-src 'self' 'unsafe-inline'; " +
            "img-src 'self' data:; " +
            "font-src 'self'; " +
            "connect-src 'self'; " +
            "frame-ancestors 'none'; " +
            "form-action 'self'; " +
            "base-uri 'self'; " +
            "object-src 'none'";

        public void Init(HttpApplication context)
        {
            if (context == null)
                throw new ArgumentNullException("context");

            context.PreSendRequestHeaders += OnPreSendRequestHeaders;
        }

        private void OnPreSendRequestHeaders(object sender, EventArgs e)
        {
            HttpApplication application = sender as HttpApplication;
            if (application == null || application.Response == null)
                return;

            HttpResponse response = application.Response;

            // Resolve the CSP policy: prefer the environment variable so that
            // AWS deployment environments can supply their own policy without
            // requiring a redeployment.
            string cspPolicy = Environment.GetEnvironmentVariable("CSP_POLICY");
            if (string.IsNullOrWhiteSpace(cspPolicy))
            {
                cspPolicy = DefaultCspPolicy;
            }

            // Set Content-Security-Policy header (remove any existing value first
            // to avoid duplicate headers that could be exploited).
            response.Headers.Remove("Content-Security-Policy");
            response.Headers.Set("Content-Security-Policy", cspPolicy);

            // Additional security headers recommended for cloud SOC2 compliance.
            response.Headers.Remove("X-Content-Type-Options");
            response.Headers.Set("X-Content-Type-Options", "nosniff");

            response.Headers.Remove("X-Frame-Options");
            response.Headers.Set("X-Frame-Options", "DENY");

            response.Headers.Remove("X-XSS-Protection");
            response.Headers.Set("X-XSS-Protection", "1; mode=block");

            response.Headers.Remove("Referrer-Policy");
            response.Headers.Set("Referrer-Policy", "strict-origin-when-cross-origin");
        }

        public void Dispose()
        {
            // No unmanaged resources to release.
        }
    }
}
