<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied:

    Line 1 – Removed: <%@ Page Title="" Language="C#" MasterPageFile="~/Patient/PatientMaster.Master"
                               AutoEventWireup="true" CodeBehind="TreatmentHistory.aspx.cs"
                               Inherits="DBProject.TreatmentHistory" %>
             (Web Forms Page directive not available in ASP.NET Core)

    All Web Forms server controls have been replaced:
      - <%@ Page %> directive with MasterPageFile, AutoEventWireup, CodeBehind, Inherits
                                                   → @page / @model directives in TreatmentHistory.cshtml
      - asp:Content ID="Content1" (head section)   → @section head { } in TreatmentHistory.cshtml
      - asp:Content ID="Content2" (body section)   → @RenderBody() content in TreatmentHistory.cshtml
      - asp:Label ID="THistory"                    → @Model.THistory rendered in TreatmentHistory.cshtml
      - asp:GridView ID="THistoryGrid"             → HTML table rendered from DataTable in TreatmentHistory.cshtml

    Rule cr-dotnet-1034 remediation applied (line 29):
      asp:GridView ID="THistoryGrid" with synchronous DataSource/DataBind pattern replaced with
      async Task-based data access in TreatmentHistory.cshtml.cs:
      - OnGet() converted to OnGetAsync() returning Task
      - LoadTreatmentHistory() converted to LoadTreatmentHistoryAsync() returning Task
      - Synchronous myDAL.getTreatmentHistory() replaced with async myDAL.GetTreatmentHistoryAsync()
        which uses SqlConnection.OpenAsync() and SqlCommand.ExecuteReaderAsync()
        connected to Amazon RDS via RDS Proxy, preventing thread pool exhaustion
        under load and enabling efficient auto-scaling in cloud deployments.

    Rule cr-dotnet-1034 remediation applied (line 55):
      asp:GridView ID="THistoryGrid" DataSource/DataBind (THistoryGrid.DataSource = DT; THistoryGrid.DataBind())
      replaced with async DataTable population via myDAL.GetTreatmentHistoryAsync() in
      TreatmentHistory.cshtml.cs, rendered as HTML table in TreatmentHistory.cshtml.

    Migration target:
      Patient/TreatmentHistory.cshtml       (ASP.NET Core Razor Pages view)
      Patient/TreatmentHistory.cshtml.cs    (ASP.NET Core Razor Pages PageModel)

    This file is retained as a migration record only and is not compiled or served.
--%>
