<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied (line 1):
      - Removed <%@ Page %> directive referencing System.Web.UI.Page, MasterPageFile,
        AutoEventWireup, CodeBehind, and Inherits attributes
      - Web Forms asp:Content, asp:Label, asp:GridView, asp:TemplateField server controls removed
      - Replaced by Patient/BillsHistory.cshtml (ASP.NET Core Razor Pages)
      - Layout provided by Patient/_PatientLayout.cshtml

    Rule cr-dotnet-1034 remediation applied (lines 25 and 51):
      Line 25: asp:GridView ID="BHistoryGrid" runat="server" – synchronous GridView control
        with DataSource/DataBind pattern removed. Replaced by async HTML table rendering in
        BillsHistory.cshtml, populated via async Task OnGetAsync() in
        BillsHistory.cshtml.cs using myDAL.GetBillHistoryAsync() with ExecuteReaderAsync()
        against Amazon RDS via RDS Proxy. Prevents thread pool exhaustion under load and
        enables efficient auto-scaling in cloud deployments on AWS ECS / Kubernetes.
      Line 51: asp:TemplateField HeaderText="No." with Container.DataItemIndex + 1 binding
        replaced by server-side row counter in BillsHistory.cshtml.

    Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
    See BillsHistory.cshtml and BillsHistory.cshtml.cs for the migrated implementation.
--%>
