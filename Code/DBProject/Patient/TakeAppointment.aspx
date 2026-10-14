<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied:

    Line 1 – Removed: <%@ Page Title="" Language="C#" MasterPageFile="~/Patient/PatientMaster.Master"
                               AutoEventWireup="true" CodeBehind="TakeAppointment.aspx.cs"
                               Inherits="DBProject.TakeAppointment" %>
             (Web Forms Page directive not available in ASP.NET Core)

    All Web Forms server controls have been replaced:
      - <%@ Page %> directive with MasterPageFile, AutoEventWireup, CodeBehind, Inherits
                                                   → @page / @model directives in TakeAppointment.cshtml
      - asp:Content ID="Content1" (head section)   → @section head { } in TakeAppointment.cshtml
      - asp:Content ID="Content2" (body section)   → @RenderBody() content in TakeAppointment.cshtml
      - asp:Label ID="TDept"                        → @Model.TDept rendered in TakeAppointment.cshtml
      - asp:GridView ID="TDeptGrid" with AutoGenerateSelectButton / OnRowCommand
                                                   → HTML table with Select links in TakeAppointment.cshtml
                                                     and OnGetSelectAsync handler in TakeAppointment.cshtml.cs

    Rule cr-dotnet-1034 remediation applied (line 27):
      asp:GridView ID="TDeptGrid" with synchronous DataSource/DataBind pattern replaced with
      async Task-based data access in TakeAppointment.cshtml.cs:
      - OnGet() converted to OnGetAsync() returning Task
      - LoadDeptInfo() converted to LoadDeptInfoAsync() returning Task
      - Synchronous myDAL.getdeptInfo() replaced with async myDAL.GetDeptInfoAsync()
        which uses SqlConnection.OpenAsync() and SqlCommand.ExecuteReaderAsync()
        connected to Amazon RDS via RDS Proxy, preventing thread pool exhaustion
        under load and enabling efficient auto-scaling in cloud deployments.

    Rule cr-dotnet-1034 remediation applied (line 56):
      asp:GridView ID="TDeptGrid" OnRowCommand="TDeptGrid_RowCommand" (Select handler)
      replaced with async OnGetSelectAsync() handler in TakeAppointment.cshtml.cs:
      - OnGetSelect() converted to OnGetSelectAsync() returning Task<IActionResult>
      - IDistributedCache.SetString() replaced with IDistributedCache.SetStringAsync()
        for non-blocking async cache writes to Amazon ElastiCache for Redis.

    Migration target:
      Patient/TakeAppointment.cshtml       (ASP.NET Core Razor Pages view)
      Patient/TakeAppointment.cshtml.cs    (ASP.NET Core Razor Pages PageModel)

    This file is retained as a migration record only and is not compiled or served.
--%>
