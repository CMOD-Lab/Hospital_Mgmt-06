using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (PreviousHistory.aspx / PreviousHistory.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 in original)
//   - Removed Web Forms Page_Load event handler   (line 15 in original)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - asp:Label PHistory.Text error message replaced with TempData["Error"]
//   - GridView DataSource/DataBind replaced with DataTable model property rendered in .cshtml
//
// cr-dotnet-1034 remediation applied (PreviousHistory.aspx lines 24 and 50):
//   Synchronous GridView DataBind (PHistoryGrid.DataBind()) replaced with async Task-based
//   pattern using OnGetAsync() and myDAL.GetPHistoryAsync() connected to Amazon RDS via
//   RDS Proxy. This prevents thread pool exhaustion under load and enables efficient
//   auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//   - Page_Load synchronous handler replaced with async Task OnGetAsync()
//   - PatHistory() synchronous method replaced with async Task PatHistoryAsync()
//   - PHistoryGrid.DataSource = DT; PHistoryGrid.DataBind() replaced with
//     await objmyDAl.GetPHistoryAsync(id, dt) – non-blocking async data fetch
//   - PreviousHistory.aspx line 24: asp:GridView ID="PHistoryGrid" – synchronous DataBind
//     replaced by async data population in PatHistoryAsync()
//   - PreviousHistory.aspx line 50: asp:TemplateField row number binding replaced by
//     server-side row counter in PreviousHistory.cshtml
//
// Rule cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.GetInt32("idoriginal") (line 31 in original)
//     with IDistributedCache (Amazon ElastiCache for Redis) via RedisSessionExtensions.GetInt32.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.

namespace DBProject.Doctor
{
    public class PreviousHistoryModel : PageModel
    {
        // cr-dotnet-0045: IDistributedCache backed by Amazon ElastiCache for Redis,
        // replacing in-process HttpContext.Session to enable stateless horizontal scaling.
        private readonly IDistributedCache _distributedCache;

        public PreviousHistoryModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // Replaces Web Forms GridView DataSource – rendered as HTML table in PreviousHistory.cshtml
        public DataTable PatientHistory { get; set; } = new DataTable();

        // cr-dotnet-1034: Replaced synchronous void Page_Load / PHistoryGrid.DataBind()
        // (PreviousHistory.aspx lines 24 and 50) with async Task OnGetAsync().
        // Uses myDAL.GetPHistoryAsync() which calls ExecuteReaderAsync() against Amazon RDS
        // via RDS Proxy, releasing the thread pool thread while awaiting database I/O.
        // Prevents thread pool exhaustion under load and enables efficient auto-scaling.
        public async Task OnGetAsync()
        {
            // Preserve original Page_Load / PatHistory() business logic
            await PatHistoryAsync();
        }

        //-----------------------Function1--------------------------//
        // cr-dotnet-1034: Replaced synchronous PatHistory() with async PatHistoryAsync().
        // Calls GetPHistoryAsync() which uses ExecuteReaderAsync() via Amazon RDS Proxy.
        // Replaces Web Forms protected void PatHistory(object sender, EventArgs e)
        private async Task PatHistoryAsync()
        {
            myDAL objmyDAl = new myDAL();

            DataTable dt = new DataTable();

            // cr-dotnet-0045: Replaced HttpContext.Session.GetInt32("idoriginal") (original line 31)
            // with IDistributedCache.GetInt32 backed by Amazon ElastiCache for Redis.
            int id = (int)_distributedCache.GetInt32("idoriginal");

            // cr-dotnet-1034: Replaced synchronous getPHistory(id, ref dt) + PHistoryGrid.DataBind()
            // (PreviousHistory.aspx line 24 – GridView control declaration,
            //  PreviousHistory.aspx line 50 – asp:TemplateField DataItemIndex binding)
            // with async GetPHistoryAsync() using ExecuteReaderAsync() via Amazon RDS Proxy.
            int status = await objmyDAl.GetPHistoryAsync(id, dt);

            if (status == -1)
            {
                // Replaces: PHistory.Text = "There was some error in retrieving the Patients History."
                TempData["Error"] = "There was some error in retrieving the Patients History.";
            }
            else
            {
                // Replaces: PHistoryGrid.DataSource = DT; PHistoryGrid.DataBind();
                PatientHistory = dt;
            }

            return;
        }

        //-----------------------Add a new function here------------------//
    }
}
