// This file has been migrated to ASP.NET Core Razor Pages.
// Rule cr-dotnet-0026 remediation applied:
//
// The Web Forms designer file (TreatmentHistory.aspx.designer.cs) is no longer needed.
// ASP.NET Core Razor Pages do not use auto-generated designer files.
//
// Web Forms server controls replaced:
//   - protected global::System.Web.UI.WebControls.Label THistory
//     → public string THistory { get; set; } in TreatmentHistoryModel (TreatmentHistory.cshtml.cs)
//   - protected global::System.Web.UI.WebControls.GridView THistoryGrid
//     → public DataTable TreatmentHistoryData { get; set; } in TreatmentHistoryModel,
//       rendered as HTML table in TreatmentHistory.cshtml
//
// Rule cr-dotnet-1034 remediation applied (line 31):
//   protected global::System.Web.UI.WebControls.GridView THistoryGrid
//   The GridView control's synchronous DataSource/DataBind pattern has been replaced with
//   async Task-based data access in TreatmentHistory.cshtml.cs:
//   - public DataTable TreatmentHistoryData { get; set; } populated via async
//     myDAL.GetTreatmentHistoryAsync() using SqlConnection.OpenAsync() and
//     SqlCommand.ExecuteReaderAsync() connected to Amazon RDS via RDS Proxy,
//     preventing thread pool exhaustion under load.
//
// Migration target:
//   Patient/TreatmentHistory.cshtml       (ASP.NET Core Razor Pages view)
//   Patient/TreatmentHistory.cshtml.cs    (ASP.NET Core Razor Pages PageModel)
//
// This file is retained as a migration record only and is not compiled or served.
