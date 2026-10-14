using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (AppointmentRequestSent.aspx / AppointmentRequestSent.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 14 in original)
//   - Removed Web Forms Page_Load event handler   (line 16 in original)
//   - Removed Web Forms Button OnClick event handler (sendARequest)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - asp:Label Message.Text replaced with TempData model-bound messages
//   - asp:Button OnClick replaced with OnPost handler
//
// Rule cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.GetString("dID") (line 28 in original)
//     with IDistributedCache (Amazon ElastiCache for Redis) via RedisSessionExtensions.
//   - Replaced in-process HttpContext.Session.GetInt32("idoriginal") (line 33 in original)
//     with IDistributedCache.GetInt32 backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.GetString("freeSlot") (line 36 in original)
//     with IDistributedCache.GetString backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.

namespace DBProject.Patient
{
    public class AppointmentRequestSentModel : PageModel
    {
        // cr-dotnet-0045: IDistributedCache backed by Amazon ElastiCache for Redis,
        // replacing in-process HttpContext.Session to enable stateless horizontal scaling.
        private readonly IDistributedCache _distributedCache;

        public AppointmentRequestSentModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // Replaces asp:Label ID="Message" – rendered in AppointmentRequestSent.cshtml
        public string StatusMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            // Preserves original Page_Load (empty) – no pre-population required
        }

        // Replaces Web Forms sendARequest Button OnClick event handler (original line 22+)
        // Handles the "Send Request" button POST action.
        public IActionResult OnPostSendRequest()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0045: Replaced HttpContext.Session.GetString("dID") (original line 28)
            // with IDistributedCache.GetString backed by Amazon ElastiCache for Redis.
            string dID1 = _distributedCache.GetString("dID");
            int dID = Convert.ToInt32(dID1);

            // cr-dotnet-0045: Replaced HttpContext.Session.GetInt32("idoriginal") (original line 33)
            // with IDistributedCache.GetInt32 backed by Amazon ElastiCache for Redis.
            int pID = (int)_distributedCache.GetInt32("idoriginal");

            // cr-dotnet-0045: Replaced HttpContext.Session.GetString("freeSlot") (original line 36)
            // with IDistributedCache.GetString backed by Amazon ElastiCache for Redis.
            string temp = _distributedCache.GetString("freeSlot");
            int freeSlot = Convert.ToInt32(temp);

            string mes = string.Empty;

            int status = objmyDAl.insertAppointment(dID, pID, freeSlot, ref mes);

            if (status == -1)
            {
                TempData["Error"] = "There was some error in sending appointment request to the Doctor.";
            }
            else
            {
                TempData["Success"] = mes;
            }

            return RedirectToPage("AppointmentRequestSent");
        }
    }
}
