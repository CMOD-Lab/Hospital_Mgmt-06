// This file has been migrated to ASP.NET Core Razor Pages Layout.
// Rule cr-dotnet-0026 remediation applied:
//
// The Web Forms Master Page code-behind (PatientMaster.Master.cs) is no longer needed.
// ASP.NET Core Razor Pages Layouts do not require a code-behind class.
//
// The PatientMaster.Master functionality has been replaced by:
//   Patient/_PatientLayout.cshtml  (ASP.NET Core Razor Pages Layout)
//
// Web Forms dependencies removed:
//   - using System.Web;                (not available in ASP.NET Core)
//   - using System.Web.UI;             (not available in ASP.NET Core)
//   - using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   - public partial class PatientMaster : System.Web.UI.MasterPage
//     (System.Web.UI.MasterPage not available in ASP.NET Core)
//   - protected void Page_Load(object sender, EventArgs e)
//     (Web Forms page lifecycle event – not applicable to Razor Pages Layouts)
//
// This file is retained as a migration record only.

using System;

namespace DBProject.Patient
{
    [Obsolete("Migrated to ASP.NET Core Razor Pages Layout. See Patient/_PatientLayout.cshtml")]
    public class PatientMaster
    {
        // No code-behind required for ASP.NET Core Razor Pages Layouts.
        // All layout logic is handled declaratively in _PatientLayout.cshtml.
    }
}
