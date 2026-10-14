using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (CurrentAppointment.aspx / CurrentAppointment.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 in original)
//   - Removed Web Forms Page_Load event handler   (line 15 in original)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//
// Rule cr-dotnet-0045 remediation applied (line 28 in original):
//   - Replaced in-process HttpContext.Session.GetInt32("idoriginal") with
//     IDistributedCache.GetInt32("idoriginal") backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor (dependency injection).
//   - This enables stateless horizontal scaling across multiple ECS tasks or
//     Kubernetes pods without server affinity.

namespace DBProject.Patient
{
    public class CurrentAppointmentModel : PageModel
    {
        // Amazon ElastiCache for Redis distributed cache – injected via DI.
        // Replaces in-process HttpContext.Session (cr-dotnet-0045).
        private readonly IDistributedCache _cache;

        public CurrentAppointmentModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Replaces asp:Label ID="Appointment" – rendered in CurrentAppointment.cshtml
        public string AppointmentMessage { get; set; } = string.Empty;

        // Replaces asp:Label ID="ADoctor" – rendered in CurrentAppointment.cshtml
        public string DoctorMessage { get; set; } = string.Empty;

        // Replaces asp:Label ID="ATimings" – rendered in CurrentAppointment.cshtml
        public string TimingsMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            // Preserves original Page_Load → appointmentToday() business logic
            LoadAppointmentToday();
        }

        private void LoadAppointmentToday()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0045 fix (original line 28):
            // Replaces: int pid = (int)Session["idoriginal"];
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis instead of
            // in-process HttpContext.Session to support stateless horizontal scaling.
            int pid = _cache.GetInt32("idoriginal") ?? 0;

            string dName = string.Empty;
            string timings = string.Empty;

            int status = objmyDAl.appointmentTodayDisplayer(pid, ref dName, ref timings);

            if (status == -1)
            {
                // Replaces: Appointment.Text = "There was some error..."
                AppointmentMessage = "There was some error in retrieving the Patient's appointment.";
            }
            else if (status == 0)
            {
                // Replaces: Appointment.Text = "You have no appointment today..."
                AppointmentMessage = "You have no appointment today with any doctor.";
            }
            else
            {
                if (status == 3)
                {
                    // Replaces: ADoctor.Text = "You had an outdated appointment..."
                    DoctorMessage = "You had an outdated appointment with Doctor " + dName + " to which he didn't respond. So that appointment is discarded.";
                    // Replaces: ATimings.Text = "The Appointment Timings were : " + timings;
                    TimingsMessage = "The Appointment Timings were : " + timings;
                    return;
                }
                else if (status == 2)
                {
                    // Replaces: ADoctor.Text = "You have sent an appointment request..."
                    DoctorMessage = "You have sent an appointment request to Doctor " + dName + " which isn't approved by him yet.";
                }
                else
                {
                    // Replaces: ADoctor.Text = "Today you have an appointment with Doctor " + dName;
                    DoctorMessage = "Today you have an appointment with Doctor " + dName;
                }

                // Replaces: ATimings.Text = "The Appointment Timings are : " + timings;
                TimingsMessage = "The Appointment Timings are : " + timings;
            }
        }
    }
}
