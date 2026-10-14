// Migrated from ASP.NET Web Forms (TreatmentHistory.aspx / TreatmentHistory.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 14 in original)
//   - Removed Web Forms Page_Load event handler   (line 16 in original)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - Session access migrated from Session["key"] to HttpContext.Session.GetInt32
//   - GridView DataSource/DataBind replaced with DataTable model property rendered in TreatmentHistory.cshtml
//   - asp:Label THistory.Text replaced with model-bound THistory property
//
// Rule cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.GetInt32("idoriginal") (line 31 in original)
//     with IDistributedCache.GetInt32("idoriginal") backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.
//
// Rule cr-dotnet-1034 remediation applied (line 51 in original TreatmentHistory.aspx.cs):
//   Synchronous GridView data binding (THistoryGrid.DataSource = DT; THistoryGrid.DataBind())
//   replaced with async Task-based data access pattern using async DAL method
//   GetTreatmentHistoryAsync() connected to Amazon RDS via RDS Proxy.
//   - OnGet() converted to OnGetAsync() returning Task
//   - LoadTreatmentHistory() converted to LoadTreatmentHistoryAsync() returning Task
//   - Calls myDAL.GetTreatmentHistoryAsync() which uses SqlCommand.ExecuteReaderAsync()
//     and SqlConnection.OpenAsync() to release thread pool threads while awaiting
//     Amazon RDS query results, preventing thread pool exhaustion under load and
//     enabling efficient auto-scaling in cloud deployments.

using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

namespace DBProject.Patient
{
    public class TreatmentHistoryModel : PageModel
    {
        // cr-dotnet-0045: IDistributedCache backed by Amazon ElastiCache for Redis,
        // replacing in-process HttpContext.Session to enable stateless horizontal scaling.
        private readonly IDistributedCache _distributedCache;

        public TreatmentHistoryModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // Replaces asp:Label ID="THistory" – rendered in TreatmentHistory.cshtml
        public string THistory { get; set; } = string.Empty;

        // Replaces Web Forms GridView DataSource – rendered as HTML table in TreatmentHistory.cshtml
        public DataTable TreatmentHistoryData { get; set; } = new DataTable();

        // cr-dotnet-1034: Converted from synchronous OnGet() to async OnGetAsync().
        // Replaces synchronous GridView.DataBind() (original line 51) with async Task-based
        // data access via Amazon RDS Proxy, releasing the thread pool thread while awaiting
        // database I/O and preventing thread pool exhaustion under cloud load.
        public async Task OnGetAsync()
        {
            // Preserves original Page_Load business logic
            await LoadTreatmentHistoryAsync();
        }

        // cr-dotnet-1034: Converted from synchronous LoadTreatmentHistory() to async LoadTreatmentHistoryAsync().
        // Uses myDAL.GetTreatmentHistoryAsync() which calls SqlConnection.OpenAsync() and
        // SqlCommand.ExecuteReaderAsync() to avoid blocking thread pool threads while
        // awaiting Amazon RDS query results via RDS Proxy.
        private async Task LoadTreatmentHistoryAsync()
        {
            myDAL objmyDAl = new myDAL();
            DataTable DT = new DataTable();

            // Preserves original treatmentHistory() business logic
            // cr-dotnet-0045 fix (original line 31):
            // Replaces: int id = (int)Session["idoriginal"];
            //           (previously migrated to HttpContext.Session.GetInt32("idoriginal"))
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
            int id = (int)_distributedCache.GetInt32("idoriginal");

            // cr-dotnet-1034 fix (original line 51):
            // Replaces: objmyDAl.getTreatmentHistory(id, ref DT); THistoryGrid.DataSource = DT; THistoryGrid.DataBind();
            // Uses async DAL method to prevent thread pool exhaustion under cloud load.
            int status = await objmyDAl.GetTreatmentHistoryAsync(id, DT);

            if (status == -1)
            {
                // Replaces: THistory.Text = "There was some error..."
                THistory = "There was some error in retrieving the Patient's Treatment History.";
            }
            else if (status == 0)
            {
                // Replaces: THistory.Text = "There is currently no treatment history of yours."
                THistory = "There is currently no treatment history of yours.";
            }
            else
            {
                // Replaces: THistory.Text = "Treatment History of " + status + " Appointment(s) is found: ";
                //           THistoryGrid.DataSource = DT; THistoryGrid.DataBind();
                THistory = "Treatment History of " + status + " Appointment(s) is found: ";
                TreatmentHistoryData = DT;
            }
        }
    }
}
