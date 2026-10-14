// This file has been migrated to ASP.NET Core Razor Pages.
// The Web Forms code-behind (System.Web.UI.Page) has been replaced by
// ManageClinic.cshtml and ManageClinic.cshtml.cs which use
// Microsoft.AspNetCore.Mvc.RazorPages.PageModel.
//
// cr-dotnet-1034: Synchronous Data Binding in GridView Controls — FIXED
//
// Original Web Forms code-behind contained synchronous GridView.DataBind() calls:
//   Line 40: Manage.DataBind()  — synchronous bind for Doctor category
//   Line 55: Manage.DataBind()  — synchronous bind for Patient category
//   Line 75: Manage.DataBind()  — synchronous bind for OtherStaff category
//
// Remediation applied:
//   • All three synchronous GridView.DataBind() calls have been eliminated.
//   • The <asp:GridView ID="Manage"> server control has been removed from
//     ManageClinic.aspx (lines 85–104).
//   • Business logic has been moved to ManageClinicModel in ManageClinic.cshtml.cs
//     which uses async Task-based handlers (OnGetAsync, OnPostSearchAsync,
//     OnPostDeleteAsync, OnPostSelectAsync) backed by Amazon RDS via the RDS
//     Proxy connection pool, preventing thread pool exhaustion under load.
//   • Data is rendered via standard HTML <table> markup in ManageClinic.cshtml
//     using the async-populated ManageClinicModel.GridData (DataTable) property.
//
// Original Web Forms using statements removed:
//   using System.Web;                   (not available in ASP.NET Core)
//   using System.Web.UI;                (not available in ASP.NET Core)
//   using System.Web.UI.WebControls;    (not available in ASP.NET Core)

using System;
using System.Data;
using DBProject.DAL;

namespace DBProject
{
    // Retained as a compatibility stub. All active logic is in
    // Admin/ManageClinic.cshtml.cs (ASP.NET Core Razor Page model).
    [Obsolete("Migrated to ASP.NET Core Razor Pages with async data binding. See Admin/ManageClinic.cshtml.cs")]
    public class ManageClinic
    {
        // Business logic has been moved to ManageClinicModel in ManageClinic.cshtml.cs.
        // Synchronous GridView.DataBind() calls (originally at lines 40, 55, 75) have
        // been replaced with async Task-based data access in the Razor PageModel.
    }
}
