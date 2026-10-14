using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (BillsHistory.aspx / BillsHistory.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 in original)
//   - Removed Web Forms Page_Load event handler   (line 15 in original)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - asp:Label BHistory.Text replaced with TempData model-bound messages
//   - asp:GridView BHistoryGrid DataSource/DataBind replaced with DataTable model property
//     rendered as HTML table in BillsHistory.cshtml
//
// Rule cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.GetInt32("idoriginal") (line 30 in original)
//     with IDistributedCache (Amazon ElastiCache for Redis) via RedisSessionExtensions.GetInt32.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.
//
// cr-dotnet-1034 remediation applied (BillsHistory.aspx lines 25 and 51):
//   Synchronous GridView DataBind (BHistoryGrid.DataBind()) replaced with async Task-based
//   pattern using OnGetAsync() and myDAL.GetBillHistoryAsync() connected to Amazon RDS via
//   RDS Proxy. This prevents thread pool exhaustion under load and enables efficient
//   auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//   - Page_Load synchronous handler replaced with async Task OnGetAsync()
//   - billHistory() synchronous method replaced with async Task LoadBillHistoryAsync()
//   - BHistoryGrid.DataSource = DT; BHistoryGrid.DataBind() replaced with
//     await objmyDAl.GetBillHistoryAsync(id, DT) – non-blocking async data fetch
//   - BillsHistory.aspx line 25: asp:GridView ID="BHistoryGrid" – synchronous DataBind
//     replaced by async data population in LoadBillHistoryAsync()
//   - BillsHistory.aspx line 51: asp:TemplateField row number binding replaced by
//     server-side row counter in BillsHistory.cshtml

namespace DBProject.Patient
{
    public class BillsHistoryModel : PageModel
    {
        // cr-dotnet-0045: IDistributedCache backed by Amazon ElastiCache for Redis,
        // replacing in-process HttpContext.Session to enable stateless horizontal scaling.
        private readonly IDistributedCache _distributedCache;

        public BillsHistoryModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // Replaces Web Forms GridView DataSource – rendered as HTML table in BillsHistory.cshtml
        public DataTable BillHistoryData { get; set; } = new DataTable();

        // cr-dotnet-1034: Replaced synchronous void OnGet() / LoadBillHistory()
        // (BillsHistory.aspx lines 25 and 51) with async Task OnGetAsync().
        // Uses myDAL.GetBillHistoryAsync() which calls ExecuteReaderAsync() against Amazon RDS
        // via RDS Proxy, releasing the thread pool thread while awaiting database I/O.
        // Prevents thread pool exhaustion under load and enables efficient auto-scaling.
        public async Task OnGetAsync()
        {
            // Preserves original Page_Load → billHistory() business logic
            await LoadBillHistoryAsync();
        }

        // cr-dotnet-1034: Replaced synchronous LoadBillHistory() with async LoadBillHistoryAsync().
        // Calls GetBillHistoryAsync() which uses ExecuteReaderAsync() via Amazon RDS Proxy.
        // Replaces Web Forms protected void billHistory(object sender, EventArgs e)
        private async Task LoadBillHistoryAsync()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-1034: DataTable populated asynchronously via GetBillHistoryAsync()
            DataTable DT = new DataTable();

            // cr-dotnet-0045: Replaced HttpContext.Session.GetInt32("idoriginal") (original line 30)
            // with IDistributedCache.GetInt32 backed by Amazon ElastiCache for Redis.
            // Preserves original: int id = (int)Session["idoriginal"];
            int id = (int)_distributedCache.GetInt32("idoriginal");

            // cr-dotnet-1034: Replaced synchronous getBillHistory(id, ref DT) + BHistoryGrid.DataBind()
            // (BillsHistory.aspx line 25 – GridView control declaration,
            //  BillsHistory.aspx line 51 – asp:TemplateField DataItemIndex binding)
            // with async GetBillHistoryAsync() using ExecuteReaderAsync() via Amazon RDS Proxy.
            int status = await objmyDAl.GetBillHistoryAsync(id, DT);

            if (status == -1)
            {
                // Replaces: BHistory.Text = "There was some error..."
                TempData["Error"] = "There was some error in retrieving the Patient's Bill History.";
            }
            else if (status == 0)
            {
                // Replaces: BHistory.Text = "There is currently no bill history of yours."
                TempData["Info"] = "There is currently no bill history of yours.";
            }
            else
            {
                // Replaces: BHistory.Text = status + " Bill(s) are found: ";
                //           BHistoryGrid.DataSource = DT;
                //           BHistoryGrid.DataBind();
                TempData["Info"] = status + " Bill(s) are found: ";
                BillHistoryData = DT;
            }
        }
    }
}
