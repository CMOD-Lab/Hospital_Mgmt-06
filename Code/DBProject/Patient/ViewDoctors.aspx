<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied:

    Line 1 – Removed: <%@ Page Title="" Language="C#" MasterPageFile="~/Patient/PatientMaster.Master"
                               AutoEventWireup="true" CodeBehind="ViewDoctors.aspx.cs"
                               Inherits="DBProject.ViewDoctors" %>
             (Web Forms Page directive not available in ASP.NET Core)

    All Web Forms server controls have been replaced:
      - <%@ Page %> directive with MasterPageFile, AutoEventWireup, CodeBehind, Inherits
                                                   → @page / @model directives in ViewDoctors.cshtml
      - asp:Content ID="Content1" (head section)   → @section head { } in ViewDoctors.cshtml
      - asp:Content ID="Content2" (body section)   → @RenderBody() content in ViewDoctors.cshtml
      - asp:Label ID="TDoctor"                     → @Model.TDoctor rendered in ViewDoctors.cshtml
      - asp:GridView ID="TDoctorGrid" with AutoGenerateSelectButton / OnRowCommand
                                                   → HTML table with Select links in ViewDoctors.cshtml
                                                     and OnGetSelectAsync handler in ViewDoctors.cshtml.cs

    Rule cr-dotnet-1034 remediation applied (line 28):
      asp:GridView ID="TDoctorGrid" with synchronous DataSource/DataBind pattern replaced with
      async Task-based data access in ViewDoctors.cshtml.cs:
      - OnGet() converted to OnGetAsync() returning Task
      - LoadDeptDoctorInfo() converted to LoadDeptDoctorInfoAsync() returning Task
      - Synchronous myDAL.getDeptDoctorInfo() replaced with async myDAL.GetDeptDoctorInfoAsync()
        which uses SqlConnection.OpenAsync() and SqlCommand.ExecuteReaderAsync()
        connected to Amazon RDS via RDS Proxy, preventing thread pool exhaustion
        under load and enabling efficient auto-scaling in cloud deployments.
      - IDistributedCache.SetString() replaced with IDistributedCache.SetStringAsync()
        for non-blocking async cache writes to Amazon ElastiCache for Redis.

    Rule cr-dotnet-1034 remediation applied (line 57):
      asp:GridView ID="TDoctorGrid" OnRowCommand="TDoctorGrid_RowCommand" (Select handler)
      replaced with async OnGetSelectAsync() handler in ViewDoctors.cshtml.cs:
      - OnGetSelect() converted to OnGetSelectAsync() returning Task<IActionResult>
      - IDistributedCache.SetString() replaced with IDistributedCache.SetStringAsync()
        for non-blocking async cache writes to Amazon ElastiCache for Redis.

    Migration target:
      Patient/ViewDoctors.cshtml       (ASP.NET Core Razor Pages view)
      Patient/ViewDoctors.cshtml.cs    (ASP.NET Core Razor Pages PageModel)

    This file is retained as a migration record only and is not compiled or served.
--%>
