// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 5  – Removed: using System.Web;                (not available in ASP.NET Core)
//   Line 6  – Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   Line 13 – Removed: public partial class BillsHistory : System.Web.UI.Page
//             (System.Web.UI.Page not available in ASP.NET Core)
//   Line 15 – Removed: protected void Page_Load(object sender, EventArgs e)
//             (Web Forms page lifecycle event – replaced by Razor Pages OnGetAsync)
//
// All business logic has been preserved and migrated to:
//   Patient/BillsHistory.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Patient/BillsHistory.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
//
// cr-dotnet-1034 remediation applied (line 50):
//   Synchronous GridView DataBind (BHistoryGrid.DataBind()) at line 50 replaced with
//   async Task-based pattern in BillsHistory.cshtml.cs. OnGetAsync() calls
//   LoadBillHistoryAsync() which calls myDAL.GetBillHistoryAsync() using ExecuteReaderAsync()
//   against Amazon RDS via RDS Proxy. This prevents thread pool exhaustion under load and
//   enables efficient auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//   - Synchronous billHistory() method (line 50) replaced with async LoadBillHistoryAsync()
//   - BHistoryGrid.DataSource = DT; BHistoryGrid.DataBind() replaced with
//     await objmyDAl.GetBillHistoryAsync(id, DT) – non-blocking async data fetch

using System;

namespace DBProject.Patient
{
    // Retained as a compatibility stub. All active logic is in
    // Patient/BillsHistory.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Patient/BillsHistory.cshtml.cs")]
    public class BillsHistory
    {
        // Business logic has been moved to BillsHistory.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // Label, GridView, Session["key"])
        // have been replaced with ASP.NET Core equivalents for cloud deployment on AWS.
        //
        // cr-dotnet-1034 fix (line 50): Synchronous BHistoryGrid.DataBind()
        // replaced with async Task OnGetAsync() + LoadBillHistoryAsync() +
        // myDAL.GetBillHistoryAsync() using ExecuteReaderAsync() via Amazon RDS Proxy
        // in BillsHistory.cshtml.cs. Prevents thread pool exhaustion under load.
    }
}
