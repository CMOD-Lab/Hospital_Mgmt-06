// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
// The Web Forms designer file (ViewDoctors.aspx.designer.cs) is no longer needed.
// ASP.NET Core Razor Pages do not use auto-generated designer files.
//
// Web Forms server controls replaced:
//   - protected global::System.Web.UI.WebControls.Label TDoctor
//     → public string TDoctor { get; set; } in ViewDoctorsModel (ViewDoctors.cshtml.cs)
//   - protected global::System.Web.UI.WebControls.GridView TDoctorGrid
//     → public DataTable Doctors { get; set; } in ViewDoctorsModel,
//       rendered as HTML table in ViewDoctors.cshtml
//
// Rule cr-dotnet-1034 remediation applied (line 31 in original ViewDoctors.aspx.designer.cs):
//   ORIGINAL CODE AT LINE 31:
//     protected global::System.Web.UI.WebControls.GridView TDoctorGrid;
//   This was the auto-generated Web Forms GridView control declaration whose synchronous
//   DataSource/DataBind pattern blocked request threads, degrading cloud scalability under load.
//
//   FIX APPLIED — Async GridView Data Binding with RDS via Entity Framework Core:
//   The GridView control's synchronous DataSource/DataBind pattern has been replaced with
//   async Task-based data access in ViewDoctors.cshtml.cs:
//   - protected global::System.Web.UI.WebControls.GridView TDoctorGrid (line 31) removed
//   - public DataTable Doctors { get; set; } populated via async myDAL.GetDeptDoctorInfoAsync()
//     using SqlConnection.OpenAsync() and SqlCommand.ExecuteReaderAsync() connected to
//     Amazon RDS via RDS Proxy, preventing thread pool exhaustion under load.
//   - DataTable Doctors is rendered as an HTML table in ViewDoctors.cshtml, replacing
//     the synchronous GridView DataSource/DataBind pattern entirely.
//
// Migration target:
//   Patient/ViewDoctors.cshtml       (ASP.NET Core Razor Pages view)
//   Patient/ViewDoctors.cshtml.cs    (ASP.NET Core Razor Pages PageModel)
//
// This file is retained as a migration record only and is not compiled or served.
