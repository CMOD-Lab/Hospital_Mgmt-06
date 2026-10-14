using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (PatientHome.aspx / PatientHome.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (line 6 in original – not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 12 in original)
//   - Removed Web Forms Page_Load event handler   (line 14 in original)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//
// Rule cr-dotnet-0045 remediation applied (line 28 in original):
//   - Replaced in-process Session["idoriginal"] (line 28) with
//     IDistributedCache.GetInt32("idoriginal") backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor (dependency injection).
//   - This enables stateless horizontal scaling across multiple ECS tasks or
//     Kubernetes pods without server affinity.

namespace DBProject.Patient
{
    public class PatientHomeModel : PageModel
    {
        // Amazon ElastiCache for Redis distributed cache – injected via DI.
        // Replaces in-process HttpContext.Session (cr-dotnet-0045).
        private readonly IDistributedCache _cache;

        public PatientHomeModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Replaces asp:Label ID="PName" – rendered in PatientHome.cshtml
        public string PName { get; set; } = string.Empty;

        // Replaces asp:Label ID="PPhone" – rendered in PatientHome.cshtml
        public string PPhone { get; set; } = string.Empty;

        // Replaces asp:Label ID="PBirthDate" – rendered in PatientHome.cshtml
        public string PBirthDate { get; set; } = string.Empty;

        // Replaces asp:Label ID="PatientAge" – rendered in PatientHome.cshtml
        public string PatientAge { get; set; } = string.Empty;

        // Replaces asp:Label ID="PAddress" – rendered in PatientHome.cshtml
        public string PAddress { get; set; } = string.Empty;

        // Replaces asp:Label ID="PGender" – rendered in PatientHome.cshtml
        public string PGender { get; set; } = string.Empty;

        // Holds any error message to display (replaces Response.Write alert script)
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            // Preserves original Page_Load → patientInfo() logic
            LoadPatientInfo();
        }

        private void LoadPatientInfo()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0045 fix (original line 28):
            // Replaces: int pid = (int)Session["idoriginal"];
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis instead of
            // in-process HttpContext.Session to support stateless horizontal scaling.
            int pid = _cache.GetInt32("idoriginal") ?? 0;

            string name      = string.Empty;
            string phone     = string.Empty;
            string address   = string.Empty;
            string birthDate = string.Empty;
            int    age       = 0;
            string gender    = string.Empty;

            int status = objmyDAl.patientInfoDisplayer(pid, ref name, ref phone, ref address, ref birthDate, ref age, ref gender);

            if (status == -1)
            {
                // Replaces: Response.Write("<script>alert('There was some error...');</script>");
                ErrorMessage = "There was some error in retrieving the Patient's Info.";
            }
            else if (status == 0)
            {
                // Replaces: PName.Text = name; PPhone.Text = phone; etc.
                PName      = name;
                PPhone     = phone;
                PBirthDate = birthDate;
                PatientAge = age.ToString();
                PAddress   = address;
                PGender    = gender;
            }
        }
    }
}
