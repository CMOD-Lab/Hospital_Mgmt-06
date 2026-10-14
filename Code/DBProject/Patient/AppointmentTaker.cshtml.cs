using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (AppointmentTaker.aspx / AppointmentTaker.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 in original)
//   - Removed Web Forms Page_Load event handler   (line 15 in original)
//   - Removed GridViewCommandEventArgs / GridView RowCommand event handler
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - Response.BufferOutput / Response.Redirect replaced with RedirectToPage
//   - GridView DataSource/DataBind replaced with DataTable model property rendered in .cshtml
//   - GridView AutoGenerateSelectButton / RowCommand replaced with OnGetSelect handler
//
// cr-dotnet-0126 remediation applied (lines 17, 33 in original source):
//   Heavy coupling to stateful IIS in-process session (Session["freeSlot"], Session["dID"])
//   replaced with IDistributedCache backed by Amazon ElastiCache for Redis.
//   This eliminates IIS sticky-session server affinity and enables stateless horizontal
//   scaling across multiple ECS tasks or Kubernetes pods without sticky-session routing.
//   Session data persists across pod restarts and scales horizontally.
//
//   Line 17 fix: Session["freeSlot"] = "" replaced with
//     IDistributedCache.SetString("freeSlot", string.Empty) backed by Amazon ElastiCache for Redis.
//   Line 33 fix: Session["dID"] read replaced with
//     IDistributedCache.GetString("dID") backed by Amazon ElastiCache for Redis.
//
// Rule cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.SetString("freeSlot", ...) (line 17 in original)
//     with IDistributedCache.SetString backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.GetString("dID") (line 33 in original)
//     with IDistributedCache.GetString backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.GetInt32("idoriginal") (line 52 in original)
//     with IDistributedCache.GetInt32 backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.SetString("freeSlot", tokens[0]) (line 57 in original)
//     with IDistributedCache.SetString backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.
//
// cr-dotnet-1034 remediation applied (AppointmentTaker.aspx lines 27 and 55):
//   Synchronous GridView DataBind (PAppointmentGrid.DataBind()) replaced with async Task-based
//   pattern using OnGetAsync() and myDAL.GetFreeSlotsAsync() connected to Amazon RDS via
//   RDS Proxy. This prevents thread pool exhaustion under load and enables efficient
//   auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//   - Page_Load synchronous handler replaced with async Task OnGetAsync()
//   - freeSlots() synchronous method replaced with async Task LoadFreeSlotsAsync()
//   - PAppointmentGrid.DataSource = DT; PAppointmentGrid.DataBind() replaced with
//     await objmyDAl.GetFreeSlotsAsync(dID, pID, DT) – non-blocking async data fetch
//   - AppointmentTaker.aspx line 27: asp:GridView ID="PAppointmentGrid" – synchronous DataBind
//     replaced by async data population in LoadFreeSlotsAsync()
//   - AppointmentTaker.aspx line 55: asp:TemplateField row number binding replaced by
//     server-side row counter in AppointmentTaker.cshtml

namespace DBProject.Patient
{
    public class AppointmentTakerModel : PageModel
    {
        // cr-dotnet-0126 / cr-dotnet-0045: IDistributedCache backed by Amazon ElastiCache for Redis,
        // replacing in-process IIS sticky-session HttpContext.Session to enable stateless
        // horizontal scaling without server affinity.
        private readonly IDistributedCache _distributedCache;

        public AppointmentTakerModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // Replaces Web Forms GridView DataSource – rendered as HTML table in AppointmentTaker.cshtml
        public DataTable FreeSlots { get; set; } = new DataTable();

        // cr-dotnet-1034: Replaced synchronous void OnGet() / LoadFreeSlots()
        // (AppointmentTaker.aspx lines 27 and 55) with async Task OnGetAsync().
        // Uses myDAL.GetFreeSlotsAsync() which calls ExecuteReaderAsync() against Amazon RDS
        // via RDS Proxy, releasing the thread pool thread while awaiting database I/O.
        // Prevents thread pool exhaustion under load and enables efficient auto-scaling.
        public async Task OnGetAsync()
        {
            // Preserves original Page_Load business logic
            // cr-dotnet-0126 / cr-dotnet-0045 (original line 17):
            // Replaces: Session["freeSlot"] = "";
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
            // Eliminates IIS sticky-session dependency; session data shared across all pods.
            await _distributedCache.SetStringAsync("freeSlot", string.Empty);
            await LoadFreeSlotsAsync();
        }

        // cr-dotnet-1034: Replaced synchronous LoadFreeSlots() with async LoadFreeSlotsAsync().
        // Calls GetFreeSlotsAsync() which uses ExecuteReaderAsync() via Amazon RDS Proxy.
        // Replaces Web Forms protected void freeSlots(object sender, EventArgs e)
        private async Task LoadFreeSlotsAsync()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-1034: DataTable populated asynchronously via GetFreeSlotsAsync()
            DataTable DT = new DataTable();

            // Preserves original freeSlots() business logic
            // cr-dotnet-0126 / cr-dotnet-0045 (original line 33):
            // Replaces: string dID1 = (string)Session["dID"];
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
            // Eliminates IIS sticky-session dependency; session data shared across all pods.
            string dID1 = await _distributedCache.GetStringAsync("dID");
            int dID = Convert.ToInt32(dID1);

            // cr-dotnet-0045: Replaced HttpContext.Session.GetInt32("idoriginal") (original line 52)
            // with IDistributedCache.GetInt32 backed by Amazon ElastiCache for Redis.
            int pID = (int)_distributedCache.GetInt32("idoriginal");

            // cr-dotnet-1034: Replaced synchronous getFreeSlots(dID, pID, ref DT) + PAppointmentGrid.DataBind()
            // (AppointmentTaker.aspx line 27 – GridView control declaration,
            //  AppointmentTaker.aspx line 55 – asp:TemplateField DataItemIndex binding)
            // with async GetFreeSlotsAsync() using ExecuteReaderAsync() via Amazon RDS Proxy.
            int status = await objmyDAl.GetFreeSlotsAsync(dID, pID, DT);

            if (status == -1)
            {
                TempData["Error"] = "There was some error in retrieving the Doctors's Free Slots.";
            }
            else if (status == 0)
            {
                TempData["Info"] = "There is currently no free slot of this doctor.";
            }
            else if (status > 0)
            {
                TempData["Info"] = "The following are the " + status + " free slots of this doctor for today :";
                // Replaces: PAppointmentGrid.DataSource = DT; PAppointmentGrid.DataBind();
                FreeSlots = DT;
            }
        }

        // Replaces Web Forms PAppointmentGrid_RowCommand "Select" handler (original line 22+)
        // The free slot value is passed as a query-string parameter from the Select link
        // in AppointmentTaker.cshtml (replacing GridView AutoGenerateSelectButton / RowCommand).
        // Preserves original logic: split appointment string on ':', store tokens[0] in session,
        // then redirect to AppointmentRequestSent.
        public async Task<IActionResult> OnGetSelectAsync(string appointment)
        {
            if (!string.IsNullOrEmpty(appointment))
            {
                string[] tokens = appointment.Split(':');
                // cr-dotnet-0126 / cr-dotnet-0045 (original line 57):
                // Replaces: Session["freeSlot"] = tokens[0];
                // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
                // Eliminates IIS sticky-session dependency; session data shared across all pods.
                await _distributedCache.SetStringAsync("freeSlot", tokens[0]);
            }

            return RedirectToPage("AppointmentRequestSent");
        }
    }
}
