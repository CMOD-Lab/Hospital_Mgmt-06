// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 6  – Removed: using System.Web;                (not available in ASP.NET Core)
//             Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   Line 14 – Removed: public partial class PreviousHistory : System.Web.UI.Page
//             (System.Web.UI.Page not available in ASP.NET Core)
//   Line 16 – Removed: protected void Page_Load(object sender, EventArgs e)
//             (Web Forms page lifecycle event – replaced by Razor Pages OnGetAsync)
//
// Rule cr-dotnet-1034 remediation applied:
//   Synchronous GridView DataBind (PHistoryGrid.DataBind()) replaced with async Task-based
//   pattern. In PreviousHistory.cshtml.cs, OnGetAsync() calls PatHistoryAsync() which calls
//   myDAL.GetPHistoryAsync() using ExecuteReaderAsync() against Amazon RDS via RDS Proxy.
//   This prevents thread pool exhaustion under load and enables efficient auto-scaling in
//   cloud deployments on AWS ECS / Kubernetes.
//
// All business logic has been preserved and migrated to:
//   Doctor/PreviousHistory.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Doctor/PreviousHistory.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.

using System;

namespace DBProject.Doctor
{
    // Retained as a compatibility stub. All active logic is in
    // Doctor/PreviousHistory.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Doctor/PreviousHistory.cshtml.cs")]
    public class PreviousHistory
    {
        // Business logic has been moved to PreviousHistory.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // Label, GridView, Session["key"], Response.Write, Response.Redirect)
        // have been replaced with ASP.NET Core equivalents for cloud deployment on AWS.
        //
        // cr-dotnet-1034 fix: Synchronous PHistoryGrid.DataBind() replaced with
        // async Task OnGetAsync() + PatHistoryAsync() + myDAL.GetPHistoryAsync()
        // in PreviousHistory.cshtml.cs. Uses ExecuteReaderAsync() via Amazon RDS Proxy
        // to prevent thread pool exhaustion under load.
    }
}
