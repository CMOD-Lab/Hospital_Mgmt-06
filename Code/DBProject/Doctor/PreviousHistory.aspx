<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied (line 1):
      - Removed <%@ Page %> directive referencing System.Web.UI.Page, MasterPageFile,
        AutoEventWireup, CodeBehind, and Inherits attributes
      - Web Forms asp:Content, asp:Label, asp:GridView, asp:TemplateField server controls removed
      - Replaced by Doctor/PreviousHistory.cshtml (ASP.NET Core Razor Pages)
      - Layout provided by Doctor/_DoctorLayout.cshtml

    Rule cr-dotnet-1034 remediation applied (lines 24 and 50):
      Line 24: asp:GridView ID="PHistoryGrid" – synchronous DataBind() replaced with
               async Task-based pattern in PreviousHistory.cshtml.cs (OnGetAsync).
               myDAL.GetPHistoryAsync() uses ExecuteReaderAsync() via Amazon RDS Proxy,
               preventing thread pool exhaustion under load and enabling auto-scaling.
      Line 50: asp:TemplateField with Container.DataItemIndex + 1 (row number binding)
               replaced by server-side integer counter in PreviousHistory.cshtml, populated
               via await objmyDAl.GetPHistoryAsync(id, dt) in PatHistoryAsync().
               PHistoryGrid.DataSource = DT; PHistoryGrid.DataBind() replaced with
               async GetPHistoryAsync() using ExecuteReaderAsync() via Amazon RDS Proxy.

    Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
    See PreviousHistory.cshtml and PreviousHistory.cshtml.cs for the migrated implementation.
--%>
