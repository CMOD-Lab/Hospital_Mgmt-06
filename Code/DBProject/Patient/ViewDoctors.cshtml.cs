// Migrated from ASP.NET Web Forms (ViewDoctors.aspx / ViewDoctors.aspx.cs)
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
//   - Session access migrated from Session["key"] to HttpContext.Session.SetString/GetString
//   - Response.BufferOutput / Response.Redirect replaced with RedirectToPage
//   - GridView DataSource/DataBind replaced with DataTable model property rendered in ViewDoctors.cshtml
//   - GridView AutoGenerateSelectButton / RowCommand replaced with OnGetSelectAsync handler
//
// cr-dotnet-0126 remediation applied (lines 17, 30 in original source):
//   Heavy coupling to stateful IIS in-process session (Session["dID"], Session["deptOriginal"])
//   replaced with IDistributedCache backed by Amazon ElastiCache for Redis in
//   ViewDoctors.cshtml.cs. This eliminates IIS sticky-session server affinity and enables
//   stateless horizontal scaling across multiple ECS tasks or Kubernetes pods without
//   sticky-session routing.
//
//   Line 17 fix: Session["dID"] = "" replaced with
//     IDistributedCache.SetStringAsync("dID", string.Empty) backed by Amazon ElastiCache for Redis.
//   Line 30 fix: Session["deptOriginal"] read replaced with
//     IDistributedCache.GetString("deptOriginal") backed by Amazon ElastiCache for Redis.
//
// cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.SetString("dID", string.Empty) (line 17 in original)
//     with IDistributedCache.SetStringAsync backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.GetString("deptOriginal") (line 30 in original)
//     with IDistributedCache.GetString backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.GetString("deptOriginal") (line 49 in original)
//     with IDistributedCache.GetString backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.SetString("dID", dID) (line 61 in original)
//     with IDistributedCache.SetStringAsync backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.
//
// Rule cr-dotnet-1034 remediation applied (lines 28 and 57 in original ViewDoctors.aspx):
//   Synchronous GridView data binding (TDoctorGrid.DataSource = DT; TDoctorGrid.DataBind())
//   replaced with async Task-based data access pattern using async DAL method
//   GetDeptDoctorInfoAsync() connected to Amazon RDS via RDS Proxy.
//   - OnGet() converted to OnGetAsync() returning Task
//   - OnGetSelect() converted to OnGetSelectAsync() returning Task<IActionResult>
//   - LoadDeptDoctorInfo() converted to LoadDeptDoctorInfoAsync() returning Task
//   - Calls myDAL.GetDeptDoctorInfoAsync() which uses SqlCommand.ExecuteReaderAsync()
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
    public class ViewDoctorsModel : PageModel
    {
        // cr-dotnet-0126 / cr-dotnet-0045: IDistributedCache backed by Amazon ElastiCache for Redis,
        // replacing in-process IIS sticky-session HttpContext.Session to eliminate server affinity
        // and enable stateless horizontal scaling.
        private readonly IDistributedCache _distributedCache;

        public ViewDoctorsModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // Replaces asp:Label ID="TDoctor" – rendered in ViewDoctors.cshtml
        public string TDoctor { get; set; } = string.Empty;

        // Replaces Web Forms GridView DataSource – rendered as HTML table in ViewDoctors.cshtml
        public DataTable Doctors { get; set; } = new DataTable();

        // cr-dotnet-1034: Converted from synchronous OnGet() to async OnGetAsync().
        // Replaces synchronous GridView.DataBind() (original ViewDoctors.aspx line 28) with
        // async Task-based data access via Amazon RDS Proxy, releasing the thread pool thread
        // while awaiting database I/O and preventing thread pool exhaustion under cloud load.
        public async Task OnGetAsync()
        {
            // Preserves original Page_Load business logic
            // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 17):
            // Replaces: Session["dID"] = "";
            //           (previously migrated to HttpContext.Session.SetString("dID", string.Empty))
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
            // Eliminates IIS sticky-session dependency; session data shared across all pods.
            await _distributedCache.SetStringAsync("dID", string.Empty);
            await LoadDeptDoctorInfoAsync();
        }

        // cr-dotnet-1034: Converted from synchronous LoadDeptDoctorInfo() to async LoadDeptDoctorInfoAsync().
        // Uses myDAL.GetDeptDoctorInfoAsync() which calls SqlConnection.OpenAsync() and
        // SqlCommand.ExecuteReaderAsync() to avoid blocking thread pool threads while
        // awaiting Amazon RDS query results via RDS Proxy.
        private async Task LoadDeptDoctorInfoAsync()
        {
            myDAL objmyDAl = new myDAL();
            DataTable DT = new DataTable();

            // Preserves original deptDoctorInfo() business logic
            // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 30):
            // Replaces: string deptName = (string)Session["deptOriginal"];
            //           (previously migrated to HttpContext.Session.GetString("deptOriginal"))
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
            // Eliminates IIS sticky-session dependency; session data shared across all pods.
            string deptName = _distributedCache.GetString("deptOriginal") ?? string.Empty;

            // cr-dotnet-1034 fix (original ViewDoctors.aspx line 57):
            // Replaces: objmyDAl.getDeptDoctorInfo(deptName, ref DT); TDoctorGrid.DataSource = DT; TDoctorGrid.DataBind();
            // Uses async DAL method to prevent thread pool exhaustion under cloud load.
            int status = await objmyDAl.GetDeptDoctorInfoAsync(deptName, DT);

            if (status == -1)
            {
                // Replaces: TDoctor.Text = "There was some error..."
                TDoctor = "There was some error in retrieving the Doctors Information.";
            }
            else
            {
                // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 49):
                // Replaces: TDoctor.Text = "Following are our Specialized Doctors of " + Session["deptOriginal"] + " Department:";
                // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
                TDoctor = "Following are our Specialized Doctors of " + deptName + " Department:";
                Doctors = DT;
            }
        }

        // cr-dotnet-1034: Converted from synchronous OnGetSelect() to async OnGetSelectAsync().
        // Replaces Web Forms TDoctorGrid_RowCommand "Select" handler.
        // The doctor ID is passed as a query-string parameter from the Select link
        // in ViewDoctors.cshtml (replacing GridView AutoGenerateSelectButton / RowCommand).
        // Preserves original logic: store dID in session, then redirect to DoctorProfile.
        public async Task<IActionResult> OnGetSelectAsync(string dID)
        {
            if (!string.IsNullOrEmpty(dID))
            {
                // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 61):
                // Replaces: Session["dID"] = dID;
                //           (previously migrated to HttpContext.Session.SetString("dID", dID))
                // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
                // Eliminates IIS sticky-session dependency; session data shared across all pods.
                await _distributedCache.SetStringAsync("dID", dID);
            }

            // Replaces: Response.BufferOutput = true; Response.Redirect("DoctorProfile.aspx");
            return RedirectToPage("DoctorProfile");
        }
    }
}
