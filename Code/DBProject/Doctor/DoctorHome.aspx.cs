// This file has been migrated to ASP.NET Core Razor Pages.
// The Web Forms code-behind (System.Web.UI.Page) has been replaced by
// DoctorHome.cshtml and DoctorHome.cshtml.cs which use
// Microsoft.AspNetCore.Mvc.RazorPages.PageModel.
//
// Original Web Forms using statements removed:
//   using System.Web;                   (not available in ASP.NET Core)
//   using System.Web.UI;                (not available in ASP.NET Core)
//   using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//
// All business logic has been preserved in DoctorHome.cshtml.cs.
// Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
//
// cr-dotnet-0045 remediation applied (line 21):
//   In-process HttpSessionState (Session["idoriginal"]) replaced with IDistributedCache
//   backed by Amazon ElastiCache for Redis in DoctorHome.cshtml.cs.
//   This eliminates server affinity and enables stateless horizontal scaling across
//   multiple ECS tasks or Kubernetes pods.
//   Redis connection is configured via the REDIS_CONNECTION_STRING environment variable
//   and registered in Program.cs via AddStackExchangeRedisCache().

using System;
using System.Data;
using DBProject.DAL;

namespace doctor
{
    // Retained as a compatibility stub. All active logic is in
    // Doctor/DoctorHome.cshtml.cs (ASP.NET Core Razor Page model).
    [Obsolete("Migrated to ASP.NET Core Razor Pages. See Doctor/DoctorHome.cshtml.cs")]
    public class doctorhome
    {
        // Business logic has been moved to DoctorHomeModel in DoctorHome.cshtml.cs.
        // Web Forms dependencies (System.Web.UI.Page, System.Web.UI.WebControls,
        // System.Web.UI.MasterPage) have been removed as they are not compatible
        // with ASP.NET Core cloud deployments on AWS.
        //
        // Session state (cr-dotnet-0045): HttpSessionState replaced with
        // IDistributedCache (Amazon ElastiCache for Redis) in DoctorHome.cshtml.cs.
    }
}
