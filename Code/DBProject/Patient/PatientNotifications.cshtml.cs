// Migrated from ASP.NET Web Forms (PatientNotifications.aspx / PatientNotifications.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 15 in original)
//   - Removed Web Forms Page_Load event handler   (line 17 in original)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//
// Rule cr-dotnet-0045 remediation applied (line 30 in original):
//   - Replaced in-process Session["idoriginal"] (line 30) with
//     IDistributedCache.GetInt32("idoriginal") backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor (dependency injection).
//   - This enables stateless horizontal scaling across multiple ECS tasks or
//     Kubernetes pods without server affinity.

using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

namespace DBProject.Patient
{
    public class PatientNotificationsModel : PageModel
    {
        // Amazon ElastiCache for Redis distributed cache – injected via DI.
        // Replaces in-process HttpContext.Session (cr-dotnet-0045).
        private readonly IDistributedCache _cache;

        public PatientNotificationsModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Replaces asp:Label ID="Notify" runat="server" – rendered in PatientNotifications.cshtml
        public string Notify { get; set; } = string.Empty;

        // Replaces asp:Label ID="NDoctor" runat="server" – rendered in PatientNotifications.cshtml
        public string NDoctor { get; set; } = string.Empty;

        // Replaces asp:Label ID="NTimings" runat="server" – rendered in PatientNotifications.cshtml
        public string NTimings { get; set; } = string.Empty;

        public void OnGet()
        {
            // Preserves original Page_Load → Notifications() business logic
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0045 fix (original line 30):
            // Replaces: int pid = (int)Session["idoriginal"];
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis instead of
            // in-process HttpContext.Session to support stateless horizontal scaling.
            int pid = _cache.GetInt32("idoriginal") ?? 0;

            string dName = string.Empty;
            string timings = string.Empty;

            int status = objmyDAl.getNotifications(pid, ref dName, ref timings);

            if (status == -1)
            {
                // Replaces: Notify.Text = "There was some error..."
                Notify = "There was some error in retrieving the Patient's notifications.";
            }
            else if (status == 0)
            {
                // Replaces: Notify.Text = "There are no new notifications :)"
                Notify = "There are no new notifications :)";
            }
            else
            {
                if (status == 1)
                {
                    // Replaces: NDoctor.Text = "Your requested appointment..."
                    NDoctor = "Your requested appointment with Doctor " + dName + " has been accepted by him! :)";
                    // Replaces: NTimings.Text = "The Appointment Timings are : " + timings
                    NTimings = "The Appointment Timings are : " + timings;
                    return;
                }
                else if (status == 2)
                {
                    NDoctor = "Your requested appointment with Doctor " + dName + " has been rejected by him! :(";
                    NTimings = "The Appointment Timings were : " + timings;
                    return;
                }
                else if (status == 3)
                {
                    NDoctor = "Your appointment with Doctor " + dName + " has been completed now. We hope you are feeling better now!";
                    NTimings = "The Appointment Timings were : " + timings;
                    return;
                }
            }
        }
    }
}
