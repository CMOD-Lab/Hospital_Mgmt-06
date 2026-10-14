// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 5  – Removed: using System.Web;                (not available in ASP.NET Core)
//   Line 6  – Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   Line 13 – Removed: public partial class pendingappointment : System.Web.UI.Page
//             (System.Web.UI.Page not available in ASP.NET Core)
//   Line 15 – Removed: protected void Page_Load(object sender, EventArgs e)
//             (Web Forms page lifecycle event – replaced by Razor Pages OnGetAsync)
//
// Rule cr-dotnet-1034 remediation applied (line 32):
//   Line 32: pendingappointments.DataSource = DT; pendingappointments.DataBind();
//            Synchronous GridView DataBind in loadgrid() replaced with async Task-based pattern.
//            In PendingAppointment.cshtml.cs, OnGetAsync() calls LoadGridAsync() which calls
//            myDAL.GetAllPendingAppointmentsAsync() using ExecuteReaderAsync() against Amazon
//            RDS via RDS Proxy. This prevents thread pool exhaustion under load and enables
//            efficient auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//            update_appointment and Delete_appointment handlers converted to async POST handlers.
//
// All business logic has been preserved and migrated to:
//   Doctor/PendingAppointment.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Doctor/PendingAppointment.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.

using System;

namespace doctor
{
    // Retained as a compatibility stub. All active logic is in
    // Doctor/PendingAppointment.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Doctor/PendingAppointment.cshtml.cs")]
    public class pendingappointment
    {
        // Business logic has been moved to PendingAppointment.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // GridView, GridViewCommandEventArgs, GridViewDeleteEventArgs,
        // Session["key"], Response.Write, Response.Redirect)
        // have been replaced with ASP.NET Core equivalents for cloud deployment on AWS.
        //
        // cr-dotnet-1034 fix (line 32): Synchronous pendingappointments.DataBind() replaced
        // with async Task OnGetAsync() + LoadGridAsync() + myDAL.GetAllPendingAppointmentsAsync()
        // in PendingAppointment.cshtml.cs. Uses ExecuteReaderAsync() via Amazon RDS Proxy
        // to prevent thread pool exhaustion under load.
    }
}
