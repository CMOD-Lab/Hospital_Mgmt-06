// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 5  – Removed: using System.Web;                (not available in ASP.NET Core)
//   Line 6  – Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   Line 12 – Removed: System.Web.UI.Page inheritance
//   Line 14 – Removed: Web Forms event handler signatures (saveindatabase, generate_bill)
//
// All business logic has been preserved and migrated to:
//   Doctor/HistoryUpdate.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Doctor/HistoryUpdate.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
//
// cr-dotnet-0045 remediation applied (lines 23, 28):
//   In-process HttpSessionState (Session["idoriginal"], Session["appointid"]) replaced
//   with IDistributedCache backed by Amazon ElastiCache for Redis in HistoryUpdate.cshtml.cs.
//   This eliminates server affinity and enables stateless horizontal scaling across
//   multiple ECS tasks or Kubernetes pods.
//   Redis connection is configured via the REDIS_CONNECTION_STRING environment variable
//   and registered in Program.cs via AddStackExchangeRedisCache().

using System;

namespace doctor
{
    // Retained as a compatibility stub. All active logic is in
    // Doctor/HistoryUpdate.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Doctor/HistoryUpdate.cshtml.cs")]
    public class Historyupdate
    {
        // Business logic has been moved to HistoryUpdate.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // Session["key"], Response.Write, Response.Redirect) have been replaced
        // with ASP.NET Core equivalents for cloud deployment on AWS.
        //
        // Session state (cr-dotnet-0045): HttpSessionState replaced with
        // IDistributedCache (Amazon ElastiCache for Redis) in HistoryUpdate.cshtml.cs.
    }
}
