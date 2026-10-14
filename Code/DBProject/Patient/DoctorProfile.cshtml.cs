using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (DoctorProfile.aspx / DoctorProfile.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 14 in original)
//   - Removed Web Forms Page_Load event handler   (line 16 in original)
//   - Removed Web Forms Button OnClick event handler (RedirectToAppointmentTaker)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//
// Rule cr-dotnet-0045 remediation applied (lines 29 and 45 in original):
//   - Replaced in-process Session["dID"] (line 29) with IDistributedCache.GetString("dID")
//     backed by Amazon ElastiCache for Redis.
//   - Replaced in-process Session["deptOriginal"] (line 45) with
//     IDistributedCache.GetString("deptOriginal") backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor (dependency injection).
//   - This enables stateless horizontal scaling across multiple ECS tasks or
//     Kubernetes pods without server affinity.

namespace DBProject.Patient
{
    public class DoctorProfileModel : PageModel
    {
        // Amazon ElastiCache for Redis distributed cache – injected via DI.
        // Replaces in-process HttpContext.Session (cr-dotnet-0045).
        private readonly IDistributedCache _cache;

        public DoctorProfileModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Replaces asp:Label ID="DName" – rendered in DoctorProfile.cshtml
        public string DName { get; set; } = string.Empty;

        // Replaces asp:Label ID="DPhone" – rendered in DoctorProfile.cshtml
        public string DPhone { get; set; } = string.Empty;

        // Replaces asp:Label ID="DQualification" – rendered in DoctorProfile.cshtml
        public string DQualification { get; set; } = string.Empty;

        // Replaces asp:Label ID="DSpecialization" – rendered in DoctorProfile.cshtml
        public string DSpecialization { get; set; } = string.Empty;

        // Replaces asp:Label ID="DWork" – rendered in DoctorProfile.cshtml
        public string DWork { get; set; } = string.Empty;

        // Replaces asp:Label ID="DAge" – rendered in DoctorProfile.cshtml
        public string DAge { get; set; } = string.Empty;

        // Replaces asp:Label ID="DGender" – rendered in DoctorProfile.cshtml
        public string DGender { get; set; } = string.Empty;

        // Replaces asp:Label ID="DDept" – rendered in DoctorProfile.cshtml
        public string DDept { get; set; } = string.Empty;

        // Replaces asp:Label ID="DCharges" – rendered in DoctorProfile.cshtml
        public string DCharges { get; set; } = string.Empty;

        // Replaces asp:Label ID="DRI" – rendered in DoctorProfile.cshtml
        public string DRI { get; set; } = string.Empty;

        // Replaces asp:Label ID="DPT" – rendered in DoctorProfile.cshtml
        public string DPT { get; set; } = string.Empty;

        // Error message for display when doctor info retrieval fails
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            // Preserves original Page_Load → doctorInfo() business logic
            LoadDoctorInfo();
        }

        private void LoadDoctorInfo()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0045 fix (original line 29):
            // Replaces: string dID1 = (string)Session["dID"];
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis instead of
            // in-process HttpContext.Session to support stateless horizontal scaling.
            string dID1 = _cache.GetString("dID") ?? string.Empty;
            int dID = Convert.ToInt32(dID1);

            string name = string.Empty;
            string phone = string.Empty;
            string gender = string.Empty;

            float charges_Per_Visit = 0;
            float ReputeIndex = 0;
            int PatientsTreated = 0;
            string qualification = string.Empty;
            string specialization = string.Empty;
            int workE = 0;
            int age = 0;

            // cr-dotnet-0045 fix (original line 45):
            // Replaces: string deptName = (string)Session["deptOriginal"];
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis instead of
            // in-process HttpContext.Session to support stateless horizontal scaling.
            string deptName = _cache.GetString("deptOriginal") ?? string.Empty;

            int status = objmyDAl.doctorInfoDisplayer(dID, ref name, ref phone, ref gender,
                ref charges_Per_Visit, ref ReputeIndex, ref PatientsTreated,
                ref qualification, ref specialization, ref workE, ref age);

            if (status == -1)
            {
                // Replaces: Response.Write("<script>alert('There was some error...');</script>");
                ErrorMessage = "There was some error in retrieving the Doctor's Info.";
            }
            else if (status == 0)
            {
                // Replaces: DName.Text = name; DPhone.Text = phone; etc.
                DName = name;
                DPhone = phone;
                DQualification = qualification;
                DSpecialization = specialization;
                DWork = workE.ToString();
                DAge = age.ToString();
                DGender = gender;
                DDept = deptName;
                DCharges = charges_Per_Visit.ToString();
                DRI = ReputeIndex.ToString();
                DPT = PatientsTreated.ToString();
            }
        }

        // Replaces Web Forms RedirectToAppointmentTaker Button OnClick handler
        public IActionResult OnPostRedirectToAppointmentTaker()
        {
            return RedirectToPage("AppointmentTaker");
        }
    }
}
