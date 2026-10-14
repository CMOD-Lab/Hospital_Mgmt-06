// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
//   Line 6  – Removed: using System.Web;                (not available in ASP.NET Core)
//             Removed: using System.Web.UI;             (not available in ASP.NET Core)
//             Removed: using System.Web.UI.WebControls; (not available in ASP.NET Core)
//   Line 15 – Removed: public partial class PatientFeedback : System.Web.UI.Page
//             (System.Web.UI.Page not available in ASP.NET Core)
//   Line 17 – Removed: protected void Page_Load(object sender, EventArgs e)
//             (Web Forms page lifecycle event – replaced by Razor Pages OnGet)
//   Line 19 – Removed: if (!IsPostBack) { ... }
//             (Web Forms postback pattern – replaced by Razor Pages GET/POST handlers)
//
// All business logic has been preserved and migrated to:
//   Patient/PatientFeedback.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
//   Patient/PatientFeedback.cshtml     (ASP.NET Core Razor Pages view)
//
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
//
// cr-dotnet-0126 remediation applied (lines 21, 56):
//   Heavy coupling to stateful IIS in-process session (Session["idoriginal"], Session["aID"])
//   replaced with IDistributedCache backed by Amazon ElastiCache for Redis in
//   PatientFeedback.cshtml.cs. This eliminates IIS sticky-session server affinity and
//   enables stateless horizontal scaling across multiple ECS tasks or Kubernetes pods.
//   Redis connection is configured via the REDIS_CONNECTION_STRING environment variable
//   and registered in Program.cs via AddStackExchangeRedisCache().

using System;

namespace DBProject.Patient
{
    // Retained as a compatibility stub. All active logic is in
    // Patient/PatientFeedback.cshtml.cs (ASP.NET Core Razor Pages PageModel).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Patient/PatientFeedback.cshtml.cs")]
    public class PatientFeedback
    {
        // Business logic has been moved to PatientFeedback.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // Label, DropDownList, Button, Session["key"])
        // have been replaced with ASP.NET Core equivalents for cloud deployment on AWS.
        //
        // cr-dotnet-0126 fix (line 21): IIS sticky-session Session["idoriginal"] read
        // replaced with IDistributedCache.GetInt32("idoriginal") backed by
        // Amazon ElastiCache for Redis in PatientFeedback.cshtml.cs.
        //
        // cr-dotnet-0126 fix (line 56): IIS sticky-session Session["aID"] = aID
        // replaced with IDistributedCache.SetInt32("aID", aID) backed by
        // Amazon ElastiCache for Redis in PatientFeedback.cshtml.cs.
        //
        // Eliminates heavy coupling to stateful IIS middleware; enables stateless
        // horizontal scaling without sticky-session routing.
        //
        // Occurrences fixed:
        //   Line 6  – using System.Web;                removed
        //   Line 6  – using System.Web.UI;             removed
        //   Line 6  – using System.Web.UI.WebControls; removed
        //   Line 15 – System.Web.UI.Page inheritance   removed
        //   Line 17 – Page_Load event handler          replaced by OnGet in PageModel
        //   Line 19 – IsPostBack check                 replaced by Razor Pages GET/POST pattern
        //   Line 21 – Session["idoriginal"]            replaced by IDistributedCache (Redis)
        //   Line 56 – Session["aID"] = aID             replaced by IDistributedCache (Redis)
    }
}
