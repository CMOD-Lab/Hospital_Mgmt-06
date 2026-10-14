<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied (line 1):
      - Removed <%@ Page %> directive referencing System.Web.UI.Page and MasterPageFile
      - Web Forms asp:Content, asp:GridView server controls removed
      - Replaced by Doctor/PatientHistory.cshtml (ASP.NET Core Razor Pages)
      - Layout provided by Doctor/_DoctorLayout.cshtml

    Rule cr-dotnet-1034 remediation applied (lines 23 and 41):
      Line 23: asp:GridView ID="patientsgrid" – synchronous DataBind() replaced with
               async Task-based pattern in PatientHistory.cshtml.cs (OnGetAsync).
               myDAL.SearchPatientAsync() uses ExecuteReaderAsync() via Amazon RDS Proxy,
               preventing thread pool exhaustion under load and enabling auto-scaling.
      Line 41: patientsgrid.DataBind() call in Page_Load replaced by async data population
               via await objmydal.SearchPatientAsync(did, dt) in OnGetAsync().

    Migration target: ASP.NET Core Razor Pages for cloud-native AWS deployment.
    See PatientHistory.cshtml and PatientHistory.cshtml.cs for the migrated implementation.
--%>
