// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 5  – Removed: using System.Web;                (not available in ASP.NET Core)
//             Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   Line 13 – Removed: public partial class AppointmentTaker : System.Web.UI.Page
//             (System.Web.UI.Page not available in ASP.NET Core)
//   Line 15 – Removed: protected void Page_Load(object sender, EventArgs e)
//             (Web Forms page lifecycle event – replaced by Razor Pages OnGetAsync)
//
// All business logic has been preserved and migrated to:
//   Patient/AppointmentTaker.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Patient/AppointmentTaker.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
//
// cr-dotnet-0126 remediation applied (lines 17, 33):
//   Heavy coupling to stateful IIS in-process session (Session["freeSlot"], Session["dID"])
//   replaced with IDistributedCache backed by Amazon ElastiCache for Redis in
//   AppointmentTaker.cshtml.cs. This eliminates IIS sticky-session server affinity and
//   enables stateless horizontal scaling across multiple ECS tasks or Kubernetes pods.
//   Redis connection is configured via the REDIS_CONNECTION_STRING environment variable
//   and registered in Program.cs via AddStackExchangeRedisCache().
//
// cr-dotnet-1034 remediation applied (line 77):
//   Synchronous GridView DataBind (PAppointmentGrid.DataBind()) at line 77 replaced with
//   async Task-based pattern in AppointmentTaker.cshtml.cs. OnGetAsync() calls
//   LoadFreeSlotsAsync() which calls myDAL.GetFreeSlotsAsync() using ExecuteReaderAsync()
//   against Amazon RDS via RDS Proxy. This prevents thread pool exhaustion under load and
//   enables efficient auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//   - Synchronous freeSlots() method (line 77) replaced with async LoadFreeSlotsAsync()
//   - PAppointmentGrid.DataSource = DT; PAppointmentGrid.DataBind() replaced with
//     await objmyDAl.GetFreeSlotsAsync(dID, pID, DT) – non-blocking async data fetch

using System;

namespace DBProject.Patient
{
    // Retained as a compatibility stub. All active logic is in
    // Patient/AppointmentTaker.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Patient/AppointmentTaker.cshtml.cs")]
    public class AppointmentTaker
    {
        // Business logic has been moved to AppointmentTaker.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // Label, GridView, GridViewCommandEventArgs, Session["key"],
        // Response.BufferOutput, Response.Redirect)
        // have been replaced with ASP.NET Core equivalents for cloud deployment on AWS.
        //
        // cr-dotnet-0126 fix (line 17): IIS sticky-session Session["freeSlot"] = ""
        // replaced with IDistributedCache.SetStringAsync("freeSlot", string.Empty) backed by
        // Amazon ElastiCache for Redis in AppointmentTaker.cshtml.cs.
        //
        // cr-dotnet-0126 fix (line 33): IIS sticky-session Session["dID"] read
        // replaced with IDistributedCache.GetStringAsync("dID") backed by
        // Amazon ElastiCache for Redis in AppointmentTaker.cshtml.cs.
        //
        // cr-dotnet-1034 fix (line 77): Synchronous PAppointmentGrid.DataBind()
        // replaced with async Task OnGetAsync() + LoadFreeSlotsAsync() +
        // myDAL.GetFreeSlotsAsync() using ExecuteReaderAsync() via Amazon RDS Proxy
        // in AppointmentTaker.cshtml.cs. Prevents thread pool exhaustion under load.
        //
        // Eliminates heavy coupling to stateful IIS middleware; enables stateless
        // horizontal scaling without sticky-session routing.
    }
}
