<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied (line 1):
      - Removed <%@ Page %> directive referencing System.Web.UI.Page, MasterPageFile,
        AutoEventWireup, CodeBehind, and Inherits attributes
      - Web Forms asp:Content, asp:GridView server controls removed
      - Replaced by Doctor/PendingAppointment.cshtml (ASP.NET Core Razor Pages)
      - Layout provided by Doctor/_DoctorLayout.cshtml

    Rule cr-dotnet-1034 remediation applied (lines 22 and 40):
      Line 22: asp:GridView ID="pendingappointments" – synchronous DataBind() replaced with
               async Task-based pattern in PendingAppointment.cshtml.cs (OnGetAsync).
               myDAL.GetAllPendingAppointmentsAsync() uses ExecuteReaderAsync() via Amazon
               RDS Proxy, preventing thread pool exhaustion under load and enabling auto-scaling.
      Line 40: pendingappointments.DataBind() call in loadgrid() replaced by async data
               population via await objDAL.GetAllPendingAppointmentsAsync(did, dt) in
               LoadGridAsync(). OnRowCommand and OnRowDeleting handlers converted to async
               POST handlers (OnPostUpdateAppointmentAsync / OnPostDeleteAppointmentAsync).

    Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
    See PendingAppointment.cshtml and PendingAppointment.cshtml.cs for the migrated implementation.
--%>
