// This file has been migrated to ASP.NET Core Razor Pages Layout.
// The Web Forms Master Page code-behind (System.Web.UI.MasterPage) has been replaced by
// Doctor/_DoctorLayout.cshtml which uses ASP.NET Core Razor Layout syntax.
//
// Original Web Forms using statements removed:
//   using System.Web;                   (not available in ASP.NET Core)
//   using System.Web.UI;                (not available in ASP.NET Core)
//   using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//
// All layout structure has been preserved in Doctor/_DoctorLayout.cshtml.
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed System.Web.UI.MasterPage inheritance (line 12 in original)
//   - Removed System.Web, System.Web.UI, System.Web.UI.WebControls using statements
//     (lines 6 and 10 in original)
//   - Replaced with ASP.NET Core Razor Pages Layout (_DoctorLayout.cshtml)

using System;

namespace doctor
{
    // Retained as a compatibility stub. All active layout is in
    // Doctor/_DoctorLayout.cshtml (ASP.NET Core Razor Layout).
    [Obsolete("Migrated to ASP.NET Core Razor Pages Layout. See Doctor/_DoctorLayout.cshtml")]
    public class doctormaster
    {
        // Layout logic has been moved to _DoctorLayout.cshtml.
        // Web Forms dependencies (System.Web.UI.MasterPage, System.Web.UI.WebControls,
        // asp:ContentPlaceHolder) have been removed as they are not compatible
        // with ASP.NET Core cloud deployments on AWS.
    }
}
