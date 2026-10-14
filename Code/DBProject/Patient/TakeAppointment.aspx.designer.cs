// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
// The Web Forms designer file (TakeAppointment.aspx.designer.cs) is no longer needed.
// ASP.NET Core Razor Pages do not use auto-generated designer files.
//
// Web Forms server controls replaced:
//   - protected global::System.Web.UI.WebControls.Label TDept
//     → public string TDept { get; set; } in TakeAppointmentModel (TakeAppointment.cshtml.cs)
//   - protected global::System.Web.UI.WebControls.GridView TDeptGrid
//     → public DataTable Departments { get; set; } in TakeAppointmentModel,
//       rendered as HTML table in TakeAppointment.cshtml
//
// Rule cr-dotnet-1034 remediation applied (line 31):
//   protected global::System.Web.UI.WebControls.GridView TDeptGrid
//   The GridView control's synchronous DataSource/DataBind pattern has been replaced with
//   async Task-based data access in TakeAppointment.cshtml.cs:
//   - public DataTable Departments { get; set; } populated via async myDAL.GetDeptInfoAsync()
//     using SqlConnection.OpenAsync() and SqlCommand.ExecuteReaderAsync() connected to
//     Amazon RDS via RDS Proxy, preventing thread pool exhaustion under load.
//
// Migration target:
//   Patient/TakeAppointment.cshtml       (ASP.NET Core Razor Pages view)
//   Patient/TakeAppointment.cshtml.cs    (ASP.NET Core Razor Pages PageModel)
//
// This file is retained as a migration record only and is not compiled or served.
