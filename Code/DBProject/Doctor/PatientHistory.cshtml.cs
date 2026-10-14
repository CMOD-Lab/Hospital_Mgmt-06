using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (PatientHistory.aspx / PatientHistory.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance
//   - Removed Web Forms GridView server control and RowCommand event handler
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - Response.Write("<script>alert(...)") replaced with TempData model-bound messages
//   - Response.Redirect replaced with RedirectToPage
//   - GridView DataSource/DataBind replaced with DataTable model property rendered in .cshtml
//
// cr-dotnet-1034 remediation applied (PatientHistory.aspx lines 23 and 41):
//   Synchronous GridView DataBind (patientsgrid.DataBind()) replaced with async Task-based
//   pattern using OnGetAsync() and myDAL.SearchPatientAsync() connected to Amazon RDS via
//   RDS Proxy. This prevents thread pool exhaustion under load and enables efficient
//   auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//   - Page_Load synchronous handler replaced with async Task OnGetAsync()
//   - patientsgrid.DataSource = dt; patientsgrid.DataBind() replaced with
//     await objmydal.SearchPatientAsync(did, dt) – non-blocking async data fetch
//   - GridView RowCommand replaced with async OnGetSelectAsync handler
//
// cr-dotnet-0126 remediation applied (line 46 in original source):
//   Heavy coupling to stateful IIS in-process session (Session["appointid"]) replaced
//   with IDistributedCache backed by Amazon ElastiCache for Redis.
//   This eliminates IIS sticky-session server affinity and enables stateless horizontal
//   scaling across multiple ECS tasks or Kubernetes pods without sticky-session routing.
//   Session data persists across pod restarts and scales horizontally.
//   Redis connection is configured via the REDIS_CONNECTION_STRING environment variable
//   and registered in Program.cs via AddStackExchangeRedisCache().
//
// cr-dotnet-0045 remediation applied:
//   In-process HttpContext.Session (InProc) replaced with IDistributedCache backed by
//   Amazon ElastiCache for Redis.  Session values are stored in Redis so that all ECS
//   tasks / Kubernetes pods share the same session state, enabling stateless horizontal
//   scaling without server affinity.
//
//   Original occurrences fixed:
//     Line 21 (source) – Session["idoriginal"]  → _cache.GetInt32("idoriginal")
//     Line 46 (source) – Session["appointid"]   → _cache.SetInt32("appointid", appointid)

namespace doctor
{
    public class PatientHistoryModel : PageModel
    {
        // IDistributedCache is injected by ASP.NET Core DI; the Redis implementation
        // is registered in Program.cs via AddStackExchangeRedisCache().
        // cr-dotnet-0126: Replaces IIS sticky-session in-process HttpContext.Session
        // with Amazon ElastiCache for Redis distributed cache to eliminate server affinity.
        private readonly IDistributedCache _cache;

        public PatientHistoryModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Replaces Web Forms GridView DataSource – rendered as HTML table in PatientHistory.cshtml
        public DataTable Appointments { get; set; } = new DataTable();

        // cr-dotnet-1034: Replaced synchronous void Page_Load / patientsgrid.DataBind()
        // (PatientHistory.aspx lines 23 and 41) with async Task OnGetAsync().
        // Uses myDAL.SearchPatientAsync() which calls ExecuteReaderAsync() against Amazon RDS
        // via RDS Proxy, releasing the thread pool thread while awaiting database I/O.
        // Prevents thread pool exhaustion under load and enables efficient auto-scaling.
        public async Task OnGetAsync()
        {
            // Preserve original Page_Load business logic
            myDAL objmydal = new myDAL();
            DataTable dt = new DataTable();

            // cr-dotnet-0126 / cr-dotnet-0045: replaced HttpContext.Session.GetInt32("idoriginal")
            // with IDistributedCache (Amazon ElastiCache for Redis) equivalent.
            // Eliminates IIS sticky-session dependency; session data shared across all pods.
            int did = _cache.GetInt32("idoriginal") ?? 0;

            // cr-dotnet-1034: Replaced synchronous search_patient_DAL(did, ref dt) + DataBind()
            // (PatientHistory.aspx line 23 – GridView control declaration,
            //  PatientHistory.aspx line 41 – patientsgrid.DataBind() call)
            // with async SearchPatientAsync() using ExecuteReaderAsync() via Amazon RDS Proxy.
            int found = await objmydal.SearchPatientAsync(did, dt);

            if (found != 1)
            {
                TempData["Error"] = "There was some error";
            }
            else
            {
                Appointments = dt;
            }
        }

        // Handles the row-select action that was previously a GridView RowCommand.
        // The appointment ID is passed as a query-string parameter from the Select link
        // in PatientHistory.cshtml (replacing GridView AutoGenerateSelectButton / RowCommand).
        // cr-dotnet-1034: Converted to async to maintain consistent async pipeline.
        public async Task<IActionResult> OnGetSelectAsync(int appointid)
        {
            // cr-dotnet-0126 / cr-dotnet-0045 (original line 46):
            // Replaces: Session["appointid"] = appointmentid;
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis instead of
            // IIS in-process sticky-session state. Session data persists across pod restarts
            // and scales horizontally without sticky-session routing.
            await _cache.SetInt32Async("appointid", appointid);
            return RedirectToPage("HistoryUpdate");
        }
    }
}
