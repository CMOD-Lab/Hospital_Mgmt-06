// Migrated from ASP.NET Web Forms (ViewDoctors.aspx / ViewDoctors.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (line 6 – not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 – not available in ASP.NET Core)
//   - Removed Web Forms Page_Load event handler   (line 15 – not available in ASP.NET Core)
//   - Removed GridViewCommandEventArgs / GridView RowCommand event handler
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - Session access migrated from Session["key"] to HttpContext.Session.SetString/GetString
//   - Response.BufferOutput / Response.Redirect replaced with RedirectToPage
//   - GridView DataSource/DataBind replaced with DataTable model property rendered in ViewDoctors.cshtml
//   - GridView AutoGenerateSelectButton / RowCommand replaced with OnGetSelectAsync handler
//
// cr-dotnet-0126 remediation applied (lines 17, 30):
//   Heavy coupling to stateful IIS in-process session (Session["dID"], Session["deptOriginal"])
//   replaced with IDistributedCache backed by Amazon ElastiCache for Redis in
//   ViewDoctors.cshtml.cs. This eliminates IIS sticky-session server affinity and enables
//   stateless horizontal scaling across multiple ECS tasks or Kubernetes pods without
//   sticky-session routing.
//
// cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.SetString("dID", string.Empty) (line 17 in original)
//     with IDistributedCache.SetStringAsync backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.GetString("deptOriginal") (line 30 in original)
//     with IDistributedCache.GetString backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.GetString("deptOriginal") (line 49 in original)
//     with IDistributedCache.GetString backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.SetString("dID", dID) (line 61 in original)
//     with IDistributedCache.SetStringAsync backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.
//
// Rule cr-dotnet-1034 remediation applied (line 63 in original ViewDoctors.aspx.cs):
//   ORIGINAL CODE AT LINE 63:
//     TDoctorGrid.DataBind();
//   This was the synchronous GridView data binding call inside deptDoctorInfo() that
//   blocked the request thread, degrading cloud scalability under load.
//
//   FIX APPLIED — Async GridView Data Binding with RDS via Entity Framework Core:
//   Synchronous GridView data binding (TDoctorGrid.DataSource = DT; TDoctorGrid.DataBind())
//   replaced with async Task-based data access pattern using async DAL method
//   GetDeptDoctorInfoAsync() connected to Amazon RDS via RDS Proxy.
//   - OnGet() converted to OnGetAsync() returning Task
//   - OnGetSelect() converted to OnGetSelectAsync() returning Task<IActionResult>
//   - LoadDeptDoctorInfo() converted to LoadDeptDoctorInfoAsync() returning Task
//   - Calls myDAL.GetDeptDoctorInfoAsync() which uses SqlCommand.ExecuteReaderAsync()
//     and SqlConnection.OpenAsync() to release thread pool threads while awaiting
//     Amazon RDS query results, preventing thread pool exhaustion under load and
//     enabling efficient auto-scaling in cloud deployments.
//
// Migration target:
//   Patient/ViewDoctors.cshtml       (ASP.NET Core Razor Pages view)
//   Patient/ViewDoctors.cshtml.cs    (ASP.NET Core Razor Pages PageModel)
//
// This file is retained as a migration record only and is not compiled or served.
