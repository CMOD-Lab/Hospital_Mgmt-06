// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 5  – Removed: using System.Web;                (not available in ASP.NET Core)
//             Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   Line 13 – Removed: public partial class CurrentAppointment : System.Web.UI.Page
//             (System.Web.UI.Page not available in ASP.NET Core)
//   Line 15 – Removed: protected void Page_Load(object sender, EventArgs e)
//             (Web Forms page lifecycle event – replaced by Razor Pages OnGet)
//
// All business logic has been preserved and migrated to:
//   Patient/CurrentAppointment.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Patient/CurrentAppointment.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.

using System;

namespace DBProject.Patient
{
    // Retained as a compatibility stub. All active logic is in
    // Patient/CurrentAppointment.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Patient/CurrentAppointment.cshtml.cs")]
    public class CurrentAppointment
    {
        // Business logic has been moved to CurrentAppointment.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // Label, Session["key"])
        // have been replaced with ASP.NET Core equivalents for cloud deployment on AWS.
    }
}
