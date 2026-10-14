using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (Bill.aspx / Bill.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
// Removed System.Web.UI.Page and System.Web.UI.WebControls dependencies
// which are not available in ASP.NET Core.
//
// cr-dotnet-0045 remediation applied:
//   In-process HttpContext.Session (InProc) replaced with IDistributedCache backed by
//   Amazon ElastiCache for Redis.  Session values are stored in Redis so that all ECS
//   tasks / Kubernetes pods share the same session state, enabling stateless horizontal
//   scaling without server affinity.
//
//   Original occurrences fixed:
//     Line 21 (source) – Session["idoriginal"]  → _cache.GetInt32("idoriginal")
//     Line 38 (source) – Session["idoriginal"]  → _cache.GetInt32("idoriginal")
//     Line 39 (source) – Session["appointid"]   → _cache.GetInt32("appointid")
//     Line 51 (source) – Session["idoriginal"]  → _cache.GetInt32("idoriginal")
//     Line 52 (source) – Session["appointid"]   → _cache.GetInt32("appointid")

namespace doctor
{
    public class BillModel : PageModel
    {
        // IDistributedCache is injected by ASP.NET Core DI; the Redis implementation
        // is registered in Program.cs via AddStackExchangeRedisCache().
        private readonly IDistributedCache _cache;

        public BillModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        public string BillAmount { get; set; } = string.Empty;

        public void OnGet()
        {
            myDAL objmyDAL = new myDAL();
            DataTable dt = new DataTable();

            // cr-dotnet-0045: replaced HttpContext.Session.GetInt32("idoriginal")
            // with IDistributedCache (Amazon ElastiCache for Redis) equivalent.
            int did = _cache.GetInt32("idoriginal") ?? 0;

            int found = objmyDAL.generate_bill_DAL(did, ref dt);

            if (found != 1)
            {
                TempData["Error"] = "There was some error";
            }
            else
            {
                BillAmount = dt.Rows[0][0].ToString();
            }
        }

        public IActionResult OnPostBillPaid()
        {
            myDAL objmyDAL = new myDAL();

            // cr-dotnet-0045: replaced HttpContext.Session.GetInt32("idoriginal") and
            // HttpContext.Session.GetInt32("appointid") with IDistributedCache (Redis).
            int did     = _cache.GetInt32("idoriginal") ?? 0;
            int appoint = _cache.GetInt32("appointid")  ?? 0;

            objmyDAL.paid_bill_DAL(did, appoint);

            return RedirectToPage("patienthistory");
        }

        public IActionResult OnPostBillUnpaid()
        {
            myDAL objmyDAL = new myDAL();

            // cr-dotnet-0045: replaced HttpContext.Session.GetInt32("idoriginal") and
            // HttpContext.Session.GetInt32("appointid") with IDistributedCache (Redis).
            int did     = _cache.GetInt32("idoriginal") ?? 0;
            int appoint = _cache.GetInt32("appointid")  ?? 0;

            objmyDAL.Unpaid_bill_DAL(did, appoint);

            return RedirectToPage("patienthistory");
        }
    }
}
