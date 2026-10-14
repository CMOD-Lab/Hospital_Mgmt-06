using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DBProject.DAL;

// Migrated from ASP.NET Web Forms (ManageClinic.aspx / ManageClinic.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
// Removed System.Web.UI.Page, System.Web.UI.WebControls, and System.Web
// dependencies which are not available in ASP.NET Core.
//
// cr-dotnet-1034: Synchronous Data Binding in GridView Controls — FIXED
//
// Original code-behind contained three synchronous GridView.DataBind() calls:
//   Line 40: Manage.DataBind() — Doctor category load
//   Line 55: Manage.DataBind() — Patient category load
//   Line 75: Manage.DataBind() — OtherStaff category load
//
// Remediation applied:
//   • All synchronous GridView.DataBind() calls replaced with async Task-based
//     data access using Amazon RDS via the RDS Proxy connection pool.
//   • OnGet() replaced with async OnGetAsync().
//   • OnPostSearch(), OnPostDelete(), OnPostSelect() replaced with async equivalents.
//   • LoadGrid() replaced with async LoadGridAsync() using non-blocking DAL calls.
//   • Data is rendered via standard HTML <table> in ManageClinic.cshtml using the
//     async-populated GridData (DataTable) property, preventing thread pool
//     exhaustion under load and enabling efficient auto-scaling.

namespace DBProject.Admin
{
    public class ManageClinicModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Category { get; set; } = "DOCTOR";

        [BindProperty(SupportsGet = true)]
        public string SearchQuery { get; set; } = "";

        public DataTable GridData { get; set; }
        public string GridCaption { get; set; } = "Doctors Table";
        public string Message { get; set; } = "";
        public string DetailHtml { get; set; } = "";

        // cr-dotnet-1034: Replaced synchronous OnGet() with async OnGetAsync().
        // The async handler releases the ASP.NET Core thread while waiting for
        // Amazon RDS query results, enabling efficient thread pool utilisation
        // under load and supporting cloud auto-scaling.
        public async Task OnGetAsync()
        {
            await LoadGridAsync(SearchQuery ?? "", Category ?? "DOCTOR");
        }

        // cr-dotnet-1034: Replaced synchronous OnPostSearch() with async equivalent.
        public async Task<IActionResult> OnPostSearchAsync(string searchQuery, string category)
        {
            SearchQuery = searchQuery ?? "";
            Category = category ?? "DOCTOR";
            await LoadGridAsync(SearchQuery, Category);
            return Page();
        }

        // cr-dotnet-1034: Replaced synchronous OnPostDelete() with async equivalent.
        public async Task<IActionResult> OnPostDeleteAsync(int id, string category)
        {
            Category = category ?? "DOCTOR";
            myDAL objDAL = new myDAL();

            if (Category == "DOCTOR")
            {
                if (await objDAL.DeleteDoctorAsync(id) == 1)
                    Message = "Doctor No: " + id + " Deleted";
                else
                    Message = "There was some error";
            }
            else if (Category == "PATIENT")
            {
                Message = "You are not Authorized to Delete a Patient";
            }
            else
            {
                if (await objDAL.DeleteStaffAsync(id) == 1)
                    Message = "Staff No: " + id + " Deleted";
                else
                    Message = "There was some Error";
            }

            await LoadGridAsync("", Category);
            return Page();
        }

        // cr-dotnet-1034: Replaced synchronous OnPostSelect() with async equivalent.
        public async Task<IActionResult> OnPostSelectAsync(int id, string category)
        {
            Category = category ?? "DOCTOR";
            myDAL objDAL = new myDAL();

            string name = "", phone = "", gender = "", address = "", bDate = "";
            float charges_Per_Visit = 0, ReputeIndex = 0;
            int PatientsTreated = 0, workE = 0, age = 0;
            string qualification = "", specialization = "";

            if (Category == "DOCTOR")
            {
                if (await objDAL.GetDoctorProfileAsync(id, ref name, ref phone, ref gender,
                    ref charges_Per_Visit, ref ReputeIndex, ref PatientsTreated,
                    ref qualification, ref specialization, ref workE, ref age) == 1)
                {
                    DetailHtml = "<p><b>Name:</b></p>" + name +
                                 " <p><b>Phone:</b></p>" + phone +
                                 "<p><b>Gender:</b></p>" + gender +
                                 "<p><b>Qualification:</b></p>" + qualification +
                                 "<p><b>Age:</b></p>" + age +
                                 "<p><b>Charges:</b></p>" + charges_Per_Visit +
                                 "<p><b>Repute Index:</b></p>" + ReputeIndex;
                }
                else
                {
                    Message = "There was some error";
                }
            }
            else if (Category == "PATIENT")
            {
                if (await objDAL.GetPatientAsync(id, ref name, ref phone, ref address,
                    ref bDate, ref age, ref gender) == 0)
                {
                    DetailHtml = "<p><b>Name:</b></p>" + name +
                                 " <p><b>Phone:</b></p>" + phone +
                                 "<p><b>Gender:</b></p>" + gender +
                                 "<p><b>Address:</b></p>" + address +
                                 "<p><b>Age:</b></p>" + age;
                }
                else
                {
                    Message = "There was some error";
                }
            }
            else
            {
                string designation = "";
                int sal = 0;
                if (await objDAL.GetStaffAsync(id, ref name, ref phone, ref address,
                    ref gender, ref designation, ref sal) == 1)
                {
                    DetailHtml = "<p><b>Name:</b></p>" + name +
                                 " <p><b>Phone:</b></p>" + phone +
                                 "<p><b>Gender:</b></p>" + gender +
                                 "<p><b>Address:</b></p>" + address +
                                 "<p><b>Salary:</b></p>" + sal;
                }
                else
                {
                    Message = "There was some error";
                }
            }

            await LoadGridAsync("", Category);
            return Page();
        }

        // cr-dotnet-1034: Replaced synchronous LoadGrid() with async LoadGridAsync().
        // Uses non-blocking async DAL methods backed by Amazon RDS via RDS Proxy,
        // releasing the thread pool thread while awaiting database I/O.
        // Eliminates the three synchronous GridView.DataBind() calls that previously
        // blocked request threads (original lines 40, 55, 75 in ManageClinic.aspx.cs).
        private async Task LoadGridAsync(string searchQuery, string category)
        {
            myDAL objmyDaL = new myDAL();
            DataTable table = new DataTable();

            if (category == "DOCTOR")
            {
                GridCaption = "Doctors Table";
                await objmyDaL.LoadDoctorAsync(table, searchQuery);
                if (table != null && table.Rows.Count > 0)
                    GridData = table;
                else
                    Message = "No Doctors to show";
            }
            else if (category == "PATIENT")
            {
                GridCaption = "Patients Table";
                await objmyDaL.LoadPatientAsync(table, searchQuery);
                if (table != null && table.Rows.Count > 0)
                    GridData = table;
                else
                    Message = "No Patients to show";
            }
            else
            {
                GridCaption = "Other Staff Table";
                await objmyDaL.LoadOtherStaffAsync(table, searchQuery);
                if (table != null && table.Rows.Count > 0)
                    GridData = table;
                else
                    Message = "No Staff Member to show";
            }
        }
    }
}
