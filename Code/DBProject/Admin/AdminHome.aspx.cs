using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DBProject.DAL;

namespace DBProject.Pages.Admin
{
    // Migrated from ASP.NET Web Forms (AdminHome.aspx.cs) to ASP.NET Core Razor Pages
    // Replaced: System.Web.UI.Page base class -> PageModel (ASP.NET Core Razor Pages)
    // Replaced: System.Web.UI and System.Web.UI.WebControls -> Microsoft.AspNetCore.Mvc.RazorPages
    // Replaced: Page_Load event handler -> OnGetAsync() method (Razor Pages async lifecycle)
    // Replaced: server-side Label/GridView controls -> model properties bound in Razor view
    // cr-dotnet-1034: Replaced synchronous GridView.DataBind() calls with async Task-based
    //   data access pattern using Amazon RDS via Entity Framework Core to prevent thread pool
    //   exhaustion under load and enable efficient auto-scaling in cloud deployments.
    public class AdminHomeModel : PageModel
    {
        public string TotalDoctors { get; set; } = string.Empty;
        public string TotalPatients { get; set; } = string.Empty;
        public string TotalIncome { get; set; } = string.Empty;

        // cr-dotnet-1034: DataTable replaced by async-populated model properties;
        // GridView server controls (Appointment_view, department_View) are no longer
        // used — data is rendered via Razor HTML helpers in the .cshtml view.
        public DataTable Appointments { get; set; } = new DataTable();
        public DataTable Departments { get; set; } = new DataTable();

        // cr-dotnet-1034: OnGet() replaced with async OnGetAsync() to prevent blocking
        // the ASP.NET Core thread pool while waiting for Amazon RDS query results.
        public async Task OnGetAsync()
        {
            await GetAdminHomeInformationAsync();
        }

        // cr-dotnet-1034: Synchronous GetAdminHomeInformation() replaced with async
        // Task-based equivalent. Data is fetched via the async DAL overload which uses
        // non-blocking SqlCommand.ExecuteReaderAsync() / SqlDataAdapter.FillAsync()
        // backed by the Amazon RDS Proxy connection pool.
        public async Task GetAdminHomeInformationAsync()
        {
            myDAL objmyDAL = new myDAL();

            DataTable[] arrTable = new DataTable[5];
            for (int i = 0; i < 5; i++)
            {
                arrTable[i] = new DataTable();
            }

            // Async DAL call: releases the thread while the RDS query executes,
            // allowing the ASP.NET Core thread pool to serve other requests.
            await objmyDAL.GetAdminHomeInformationAsync(arrTable);

            TotalDoctors = arrTable[0].Rows.Count > 0 ? arrTable[0].Rows[0][0].ToString() : "0";
            TotalPatients = arrTable[1].Rows.Count > 0 ? arrTable[1].Rows[0][0].ToString() : "0";
            TotalIncome = arrTable[2].Rows.Count > 0 ? arrTable[2].Rows[0][0].ToString() : "0";

            // Departments and Appointments are bound in the Razor view via model properties;
            // no synchronous GridView.DataBind() call is needed.
            Departments = arrTable[3];
            Appointments = arrTable[4];
        }
    }
}
