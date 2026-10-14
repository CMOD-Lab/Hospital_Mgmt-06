using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (DoctorHome.aspx / DoctorHome.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
// Removed System.Web.UI.Page, System.Web.UI.WebControls, and System.Web
// dependencies which are not available in ASP.NET Core.
//
// Original Web Forms using statements removed:
//   using System.Web;                   (not available in ASP.NET Core)
//   using System.Web.UI;                (not available in ASP.NET Core)
//   using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//
// cr-dotnet-0045 remediation applied:
//   In-process HttpContext.Session (InProc) replaced with IDistributedCache backed by
//   Amazon ElastiCache for Redis.  Session values are stored in Redis so that all ECS
//   tasks / Kubernetes pods share the same session state, enabling stateless horizontal
//   scaling without server affinity.
//
//   Original occurrence fixed:
//     Line 21 (source) – Session["idoriginal"]  → _cache.GetInt32("idoriginal")
//
// Response.Write("<script>alert(...)") replaced with model-bound ErrorMessage property.

namespace doctor
{
    public class DoctorHomeModel : PageModel
    {
        // IDistributedCache is injected by ASP.NET Core DI; the Redis implementation
        // is registered in Program.cs via AddStackExchangeRedisCache().
        private readonly IDistributedCache _cache;

        public DoctorHomeModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Profile display properties (replacing Web Forms Label controls)
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string DepartmentNo { get; set; } = string.Empty;
        public string ChargesPerVisit { get; set; } = string.Empty;
        public string MonthlySalary { get; set; } = string.Empty;
        public string ReputeIndex { get; set; } = string.Empty;
        public string PatientsTreated { get; set; } = string.Empty;
        public string Qualification { get; set; } = string.Empty;
        public string Specialization { get; set; } = string.Empty;
        public string WorkExperience { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        public void OnGet()
        {
            myDAL objmyDAL = new myDAL();
            DataTable dt = new DataTable();

            // cr-dotnet-0045: replaced HttpContext.Session.GetInt32("idoriginal")
            // with IDistributedCache (Amazon ElastiCache for Redis) equivalent.
            int did = _cache.GetInt32("idoriginal") ?? 0;

            int found = objmyDAL.docinfo_DAL(did, ref dt);

            if (found != 1)
            {
                ErrorMessage = "There was some error";
            }
            else
            {
                Name            = dt.Rows[0][1].ToString();
                Phone           = dt.Rows[0][2].ToString();
                Address         = dt.Rows[0][3].ToString();
                BirthDate       = dt.Rows[0][4].ToString();
                Gender          = dt.Rows[0][5].ToString();
                DepartmentNo    = dt.Rows[0][6].ToString();
                ChargesPerVisit = dt.Rows[0][7].ToString();
                MonthlySalary   = dt.Rows[0][8].ToString();
                ReputeIndex     = dt.Rows[0][9].ToString();
                PatientsTreated = dt.Rows[0][10].ToString();
                Qualification   = dt.Rows[0][11].ToString();
                Specialization  = dt.Rows[0][12].ToString();
                WorkExperience  = dt.Rows[0][13].ToString();
                Status          = dt.Rows[0][14].ToString();
            }
        }
    }
}
