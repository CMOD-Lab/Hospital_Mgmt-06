using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (PendingAppointment.aspx / PendingAppointment.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 in original)
//   - Removed Web Forms Page_Load event handler   (line 15 in original)
//   - Removed GridViewCommandEventArgs / GridViewDeleteEventArgs event handlers
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - GridView DataSource/DataBind replaced with DataTable model property rendered in .cshtml
//   - GridView RowCommand (update_appointment) replaced with OnPostUpdateAppointmentAsync handler
//   - GridView RowDeleting (Delete_appointment) replaced with OnPostDeleteAppointmentAsync handler
//
// cr-dotnet-1034 remediation applied (PendingAppointment.aspx lines 22 and 40):
//   Synchronous GridView DataBind (pendingappointments.DataBind()) replaced with async
//   Task-based pattern using OnGetAsync() and myDAL.GetAllPendingAppointmentsAsync()
//   connected to Amazon RDS via RDS Proxy. This prevents thread pool exhaustion under load
//   and enables efficient auto-scaling in cloud deployments on AWS ECS / Kubernetes.
//   - Page_Load synchronous handler replaced with async Task OnGetAsync()
//   - loadgrid() synchronous method replaced with async Task LoadGridAsync()
//   - pendingappointments.DataSource = DT; pendingappointments.DataBind() replaced with
//     await objDAL.GetAllPendingAppointmentsAsync(did, dt) – non-blocking async data fetch
//   - update_appointment / Delete_appointment handlers converted to async POST handlers
//
// Rule cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.GetInt32("idoriginal") (line 25 in original)
//     with IDistributedCache (Amazon ElastiCache for Redis) via RedisSessionExtensions.GetInt32.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.

namespace doctor
{
    public class PendingAppointmentModel : PageModel
    {
        // cr-dotnet-0045: IDistributedCache backed by Amazon ElastiCache for Redis,
        // replacing in-process HttpContext.Session to enable stateless horizontal scaling.
        private readonly IDistributedCache _distributedCache;

        public PendingAppointmentModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // Replaces Web Forms GridView DataSource – rendered as HTML table in PendingAppointment.cshtml
        public DataTable PendingAppointments { get; set; } = new DataTable();

        // cr-dotnet-1034: Replaced synchronous void Page_Load / pendingappointments.DataBind()
        // (PendingAppointment.aspx lines 22 and 40) with async Task OnGetAsync().
        // Uses myDAL.GetAllPendingAppointmentsAsync() which calls ExecuteReaderAsync() against
        // Amazon RDS via RDS Proxy, releasing the thread pool thread while awaiting database I/O.
        // Prevents thread pool exhaustion under load and enables efficient auto-scaling.
        public async Task OnGetAsync()
        {
            // Preserve original Page_Load / loadgrid() business logic
            await LoadGridAsync();
        }

        // cr-dotnet-1034: Replaced synchronous loadgrid() with async LoadGridAsync().
        // Calls GetAllPendingAppointmentsAsync() which uses ExecuteReaderAsync() via RDS Proxy.
        private async Task LoadGridAsync()
        {
            myDAL objDAL = new myDAL();

            // cr-dotnet-0045: Replaced HttpContext.Session.GetInt32("idoriginal") (original line 25)
            // with IDistributedCache.GetInt32 backed by Amazon ElastiCache for Redis.
            int did = (int)_distributedCache.GetInt32("idoriginal");

            DataTable dt = new DataTable();

            // cr-dotnet-1034: Replaced synchronous GetAllpendingappointments_DAL(did, ref DT)
            // + pendingappointments.DataBind() (PendingAppointment.aspx line 22 – GridView
            // control declaration, line 40 – DataBind() call) with async
            // GetAllPendingAppointmentsAsync() using ExecuteReaderAsync() via Amazon RDS Proxy.
            await objDAL.GetAllPendingAppointmentsAsync(did, dt);

            PendingAppointments = dt;
        }

        // cr-dotnet-1034: Replaces Web Forms update_appointment GridViewCommandEventArgs handler.
        // Converted to async to maintain consistent async pipeline and prevent thread blocking.
        // Handles the "Select" row command – updates the appointment status.
        public async Task<IActionResult> OnPostUpdateAppointmentAsync(int appointmentId)
        {
            myDAL objmyDAL = new myDAL();

            // cr-dotnet-1034: Replaced synchronous UpdateAppointment_DAL with async version.
            int result = await objmyDAL.UpdateAppointmentAsync(appointmentId);

            if (result != 1)
            {
                TempData["Error"] = "There was some error updating the appointment.";
            }
            else
            {
                TempData["Success"] = "Appointment updated successfully.";
            }

            return RedirectToPage("PendingAppointment");
        }

        // cr-dotnet-1034: Replaces Web Forms Delete_appointment GridViewDeleteEventArgs handler.
        // Converted to async to maintain consistent async pipeline and prevent thread blocking.
        // Handles the delete row action.
        public async Task<IActionResult> OnPostDeleteAppointmentAsync(int appointmentId)
        {
            myDAL objDAL = new myDAL();

            // cr-dotnet-1034: Replaced synchronous Deleteappointment_DAL with async version.
            int deleteResult = await objDAL.DeleteAppointmentAsync(appointmentId);

            if (deleteResult == 1)
            {
                TempData["Success"] = "Appointment deleted successfully.";
            }
            else
            {
                TempData["Error"] = "There was some error deleting the appointment.";
            }

            return RedirectToPage("PendingAppointment");
        }
    }
}
