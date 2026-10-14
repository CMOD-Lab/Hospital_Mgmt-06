// Migrated from ASP.NET Web Forms (TakeAppointment.aspx / TakeAppointment.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance
//   - Removed Web Forms Page_Load event handler
//   - Removed GridViewCommandEventArgs / GridView RowCommand event handler
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - Response.BufferOutput / Response.Redirect replaced with RedirectToPage
//   - GridView DataSource/DataBind replaced with DataTable model property rendered in TakeAppointment.cshtml
//   - GridView AutoGenerateSelectButton / RowCommand replaced with OnGetSelectAsync handler
//
// cr-dotnet-0126 remediation applied (lines 17, 31 in original source):
//   Heavy coupling to stateful IIS in-process session (Session["deptOriginal"]) replaced
//   with IDistributedCache backed by Amazon ElastiCache for Redis.
//   This eliminates IIS sticky-session server affinity and enables stateless horizontal
//   scaling across multiple ECS tasks or Kubernetes pods without sticky-session routing.
//   Session data persists across pod restarts and scales horizontally.
//
//   Line 17 fix: Session["deptOriginal"] = "" replaced with
//     IDistributedCache.SetStringAsync("deptOriginal", string.Empty) backed by Amazon ElastiCache for Redis.
//   Line 31 fix: Session["deptOriginal"] = deptName replaced with
//     IDistributedCache.SetStringAsync("deptOriginal", deptName) backed by Amazon ElastiCache for Redis.
//
// Rule cr-dotnet-0045 remediation applied (line 17 in original):
//   - Replaced in-process Session["deptOriginal"] = "" (line 17) with
//     IDistributedCache.SetStringAsync("deptOriginal", "") backed by Amazon ElastiCache for Redis.
//   - Replaced in-process Session["deptOriginal"] = deptName in RowCommand handler with
//     IDistributedCache.SetStringAsync("deptOriginal", deptName) backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor (dependency injection).
//   - This enables stateless horizontal scaling across multiple ECS tasks or
//     Kubernetes pods without server affinity.
//
// Rule cr-dotnet-1034 remediation applied (line 63 in original TakeAppointment.aspx.cs):
//   Synchronous GridView data binding (TDeptGrid.DataSource = DT; TDeptGrid.DataBind())
//   replaced with async Task-based data access pattern using async DAL method
//   GetDeptInfoAsync() connected to Amazon RDS via RDS Proxy.
//   - OnGet() converted to OnGetAsync() returning Task
//   - OnGetSelect() converted to OnGetSelectAsync() returning Task<IActionResult>
//   - LoadDeptInfo() converted to LoadDeptInfoAsync() returning Task
//   - Calls myDAL.GetDeptInfoAsync() which uses SqlCommand.ExecuteReaderAsync()
//     and SqlConnection.OpenAsync() to release thread pool threads while awaiting
//     Amazon RDS query results, preventing thread pool exhaustion under load and
//     enabling efficient auto-scaling in cloud deployments.

using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

namespace DBProject.Patient
{
    public class TakeAppointmentModel : PageModel
    {
        // cr-dotnet-0126 / cr-dotnet-0045: Amazon ElastiCache for Redis distributed cache – injected via DI.
        // Replaces in-process IIS sticky-session HttpContext.Session to eliminate server affinity
        // and enable stateless horizontal scaling.
        private readonly IDistributedCache _cache;

        public TakeAppointmentModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Replaces asp:Label ID="TDept" – rendered in TakeAppointment.cshtml
        public string TDept { get; set; } = string.Empty;

        // Replaces Web Forms GridView DataSource – rendered as HTML table in TakeAppointment.cshtml
        public DataTable Departments { get; set; } = new DataTable();

        // cr-dotnet-1034: Converted from synchronous OnGet() to async OnGetAsync().
        // Replaces synchronous GridView.DataBind() (original line 63) with async Task-based
        // data access via Amazon RDS Proxy, releasing the thread pool thread while awaiting
        // database I/O and preventing thread pool exhaustion under cloud load.
        public async Task OnGetAsync()
        {
            // Preserves original Page_Load business logic:
            //   Session["deptOriginal"] = "";
            //   deptInfo(sender, e);
            //
            // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 17):
            // Replaces: Session["deptOriginal"] = "";
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
            // Eliminates IIS sticky-session dependency; session data shared across all pods.
            await _cache.SetStringAsync("deptOriginal", string.Empty);
            await LoadDeptInfoAsync();
        }

        // cr-dotnet-1034: Converted from synchronous LoadDeptInfo() to async LoadDeptInfoAsync().
        // Uses myDAL.GetDeptInfoAsync() which calls SqlConnection.OpenAsync() and
        // SqlCommand.ExecuteReaderAsync() to avoid blocking thread pool threads while
        // awaiting Amazon RDS query results via RDS Proxy.
        private async Task LoadDeptInfoAsync()
        {
            myDAL objmyDAl = new myDAL();
            DataTable DT = new DataTable();

            // cr-dotnet-1034 fix (original line 63):
            // Replaces: objmyDAl.getdeptInfo(ref DT); TDeptGrid.DataSource = DT; TDeptGrid.DataBind();
            // Uses async DAL method to prevent thread pool exhaustion under cloud load.
            int status = await objmyDAl.GetDeptInfoAsync(DT);

            if (status == -1)
            {
                // Replaces: TDept.Text = "There was some error..."
                TDept = "There was some error in retrieving the Departments Information.";
            }
            else
            {
                // Replaces: TDept.Text = "Following are the departments..."
                //           TDeptGrid.DataSource = DT; TDeptGrid.DataBind();
                TDept = "Following are the departments available at our Clinic : ";
                Departments = DT;
            }
        }

        // cr-dotnet-1034: Converted from synchronous OnGetSelect() to async OnGetSelectAsync().
        // Replaces Web Forms TDeptGrid_RowCommand "Select" handler.
        // The department name is passed as a query-string parameter from the Select link
        // in TakeAppointment.cshtml (replacing GridView AutoGenerateSelectButton / RowCommand).
        // Preserves original logic: store deptName in session, then redirect to ViewDoctors.
        public async Task<IActionResult> OnGetSelectAsync(string deptName)
        {
            if (!string.IsNullOrEmpty(deptName))
            {
                // cr-dotnet-0126 / cr-dotnet-0045 (original line 31):
                // Replaces: Session["deptOriginal"] = deptName;
                // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
                // Eliminates IIS sticky-session dependency; session data shared across all pods.
                await _cache.SetStringAsync("deptOriginal", deptName);
            }

            return RedirectToPage("ViewDoctors");
        }
    }
}
