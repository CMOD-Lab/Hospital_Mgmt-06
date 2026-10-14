// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 6  – Removed: using System.Web;                (not available in ASP.NET Core)
//   Line 13 – Removed: public partial class patienthistory : System.Web.UI.Page
//             (System.Web.UI.Page not available in ASP.NET Core)
//   Line 15 – Removed: protected void Page_Load(object sender, EventArgs e)
//             (Web Forms page lifecycle event – replaced by Razor Pages OnGetAsync)
//
//   Additional removals:
//             Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//
// Rule cr-dotnet-1034 remediation applied (line 29):
//   Line 29: patientsgrid.DataSource = dt; patientsgrid.DataBind();
//            Synchronous GridView DataBind replaced with async Task-based pattern.
//            In PatientHistory.cshtml.cs, OnGetAsync() calls myDAL.SearchPatientAsync()
//            which uses ExecuteReaderAsync() against Amazon RDS via RDS Proxy.
//            This prevents thread pool exhaustion under load and enables efficient
//            auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//
// All business logic has been preserved and migrated to:
//   Doctor/PatientHistory.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Doctor/PatientHistory.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
//
// cr-dotnet-0126 remediation applied (line 46):
//   Heavy coupling to stateful IIS in-process session (Session["appointid"]) replaced
//   with IDistributedCache backed by Amazon ElastiCache for Redis in PatientHistory.cshtml.cs.
//   This eliminates IIS sticky-session server affinity and enables stateless horizontal
//   scaling across multiple ECS tasks or Kubernetes pods.
//   Redis connection is configured via the REDIS_CONNECTION_STRING environment variable
//   and registered in Program.cs via AddStackExchangeRedisCache().
//
// cr-dotnet-0045 remediation applied (lines 21, 46):
//   In-process HttpSessionState (Session["idoriginal"], Session["appointid"]) replaced
//   with IDistributedCache backed by Amazon ElastiCache for Redis in PatientHistory.cshtml.cs.
//   This eliminates server affinity and enables stateless horizontal scaling across
//   multiple ECS tasks or Kubernetes pods.
//   Redis connection is configured via the REDIS_CONNECTION_STRING environment variable
//   and registered in Program.cs via AddStackExchangeRedisCache().

using System;

namespace doctor
{
    // Retained as a compatibility stub. All active logic is in
    // Doctor/PatientHistory.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Doctor/PatientHistory.cshtml.cs")]
    public class patienthistory
    {
        // Business logic has been moved to PatientHistory.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // GridView, RowCommand, Session["key"], Response.Write, Response.Redirect)
        // have been replaced with ASP.NET Core equivalents for cloud deployment on AWS.
        //
        // cr-dotnet-1034 fix (line 29): Synchronous patientsgrid.DataBind() replaced with
        // async Task OnGetAsync() + myDAL.SearchPatientAsync() in PatientHistory.cshtml.cs.
        // Uses ExecuteReaderAsync() via Amazon RDS Proxy to prevent thread pool exhaustion.
        //
        // cr-dotnet-0126 fix (line 46): IIS sticky-session Session["appointid"] replaced
        // with IDistributedCache (Amazon ElastiCache for Redis) in PatientHistory.cshtml.cs.
        // Eliminates heavy coupling to stateful IIS middleware; enables stateless
        // horizontal scaling without sticky-session routing.
        //
        // cr-dotnet-0045 fix: HttpSessionState replaced with
        // IDistributedCache (Amazon ElastiCache for Redis) in PatientHistory.cshtml.cs.
    }
}
