// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 5  – Removed: using System.Web;                (not available in ASP.NET Core)
//   Line 6  – Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   Line 12 – Removed: public partial class PatientHome : System.Web.UI.Page
//             (System.Web.UI.Page not available in ASP.NET Core)
//   Line 14 – Removed: protected void Page_Load(object sender, EventArgs e)
//             (Web Forms page lifecycle event – replaced by Razor Pages OnGet)
//
// All business logic has been preserved and migrated to:
//   Patient/PatientHome.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Patient/PatientHome.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.

using System;

namespace DBProject.Patient
{
    // Retained as a compatibility stub. All active logic is in
    // Patient/PatientHome.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Patient/PatientHome.cshtml.cs")]
    public class PatientHome
    {
        // Business logic has been moved to PatientHome.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // Label controls PName/PPhone/PBirthDate/PatientAge/PAddress/PGender,
        // Session["key"], Response.Write)
        // have been replaced with ASP.NET Core equivalents for cloud deployment on AWS.
        //
        // Occurrences fixed:
        //   Line 5  – using System.Web;                removed
        //   Line 6  – using System.Web.UI;             removed
        //   Line 6  – using System.Web.UI.WebControls; removed
        //   Line 12 – System.Web.UI.Page inheritance   removed
        //   Line 14 – Page_Load event handler          replaced by OnGet in PageModel
    }
}
