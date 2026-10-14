<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied (line 1):
      - Removed <%@ Page %> directive referencing System.Web.UI.Page, MasterPageFile,
        AutoEventWireup, CodeBehind, and Inherits attributes
      - Web Forms asp:Content, asp:Label, asp:GridView, asp:TemplateField server controls removed
      - Replaced by Patient/AppointmentTaker.cshtml (ASP.NET Core Razor Pages)
      - Layout provided by Patient/_PatientLayout.cshtml

    Rule cr-dotnet-1034 remediation applied (lines 27 and 55):
      Line 27: asp:GridView ID="PAppointmentGrid" runat="server" – synchronous GridView control
        with DataSource/DataBind pattern removed. Replaced by async HTML table rendering in
        AppointmentTaker.cshtml, populated via async Task OnGetAsync() in
        AppointmentTaker.cshtml.cs using myDAL.GetFreeSlotsAsync() with ExecuteReaderAsync()
        against Amazon RDS via RDS Proxy. Prevents thread pool exhaustion under load and
        enables efficient auto-scaling in cloud deployments on AWS ECS / Kubernetes.
      Line 55: asp:TemplateField HeaderText="No." with Container.DataItemIndex + 1 binding
        replaced by server-side row counter in AppointmentTaker.cshtml.

    Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
    See AppointmentTaker.cshtml and AppointmentTaker.cshtml.cs for the migrated implementation.
--%>
