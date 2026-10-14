// Migrated from ASP.NET Web Forms (SignUp.aspx / SignUp.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (line 6 – not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 – not available in ASP.NET Core)
//   - Removed Web Forms Page_Load event handler   (line 15 – not available in ASP.NET Core)
//   - Removed Web Forms loginV / signupV event handlers (Button.Click pattern)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern (SignUp.cshtml.cs)
//   - Session access migrated from Session["key"] to HttpContext.Session.SetInt32/GetInt32
//   - Response.BufferOutput / Response.Redirect replaced with RedirectToPage
//   - Response.Write("<script>alert(...)") replaced with TempData error messages
//   - Request.Form["Gender"] replaced with [BindProperty] model binding
//   - asp:TextBox / asp:Button server controls replaced with standard HTML form elements
//     bound via [BindProperty] in the PageModel
//
// cr-dotnet-0126 remediation applied (line 17):
//   Heavy coupling to stateful IIS in-process session (Session["idoriginal"]) replaced
//   with IDistributedCache backed by Amazon ElastiCache for Redis in SignUp.cshtml.cs.
//   This eliminates IIS sticky-session server affinity and enables stateless horizontal
//   scaling across multiple ECS tasks or Kubernetes pods without sticky-session routing.
//
//   Line 17 fix: Session["idoriginal"] = "" replaced with
//     IDistributedCache.SetString("idoriginal", string.Empty) backed by Amazon ElastiCache for Redis.
//
// cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.SetString("idoriginal", string.Empty) (line 17 in original)
//     with IDistributedCache.SetString backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.SetInt32("idoriginal", id) (line 36 in original)
//     with IDistributedCache.SetInt32 backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.SetInt32("idoriginal", id) (line 109 in original)
//     with IDistributedCache.SetInt32 backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.
//
// Migration target:
//   SignUp.cshtml       (ASP.NET Core Razor Pages view)
//   SignUp.cshtml.cs    (ASP.NET Core Razor Pages PageModel)
//
// This file is retained as a migration record only and is not compiled or served.
