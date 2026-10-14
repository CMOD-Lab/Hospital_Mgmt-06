using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (HistoryUpdate.aspx / HistoryUpdate.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 12 in original)
//   - Removed Web Forms event handler signatures  (line 14 in original – saveindatabase/generate_bill)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - Response.Write("<script>alert(...)") replaced with TempData model-bound messages
//   - Response.Redirect replaced with RedirectToPage
//   - Web Forms TextBox server controls replaced with bound model properties
//
// cr-dotnet-0045 remediation applied:
//   In-process HttpContext.Session (InProc) replaced with IDistributedCache backed by
//   Amazon ElastiCache for Redis.  Session values are stored in Redis so that all ECS
//   tasks / Kubernetes pods share the same session state, enabling stateless horizontal
//   scaling without server affinity.
//
//   Original occurrences fixed:
//     Line 23 (source) – Session["idoriginal"]  → _cache.GetInt32("idoriginal")
//     Line 28 (source) – Session["appointid"]   → _cache.GetInt32("appointid")

namespace doctor
{
    public class HistoryUpdateModel : PageModel
    {
        // IDistributedCache is injected by ASP.NET Core DI; the Redis implementation
        // is registered in Program.cs via AddStackExchangeRedisCache().
        private readonly IDistributedCache _cache;

        public HistoryUpdateModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Bound properties replacing Web Forms TextBox server controls
        [BindProperty]
        public string Disease { get; set; } = string.Empty;

        [BindProperty]
        public string Progress { get; set; } = string.Empty;

        [BindProperty]
        public string Prescription { get; set; } = string.Empty;

        public void OnGet()
        {
            // Page initialisation – no pre-population required per original Web Forms Page_Load
        }

        // Replaces Web Forms saveindatabase event handler (original line 14)
        public IActionResult OnPostSaveInDatabase()
        {
            myDAL objmyDAL = new myDAL();

            // cr-dotnet-0045: replaced HttpContext.Session.GetInt32("idoriginal") and
            // HttpContext.Session.GetInt32("appointid") with IDistributedCache (Redis).
            int did   = _cache.GetInt32("idoriginal") ?? 0;
            int appid = _cache.GetInt32("appointid")  ?? 0;

            int found = objmyDAL.update_prescription_DAL(did, appid, Disease, Progress, Prescription);

            if (found != 1)
            {
                TempData["Error"] = "There was some error";
            }
            else
            {
                TempData["Success"] = "Information Successfully Updated";
            }

            return RedirectToPage("HistoryUpdate");
        }

        // Replaces Web Forms generate_bill event handler (original line 14)
        public IActionResult OnPostGenerateBill()
        {
            return RedirectToPage("Bill");
        }
    }
}
