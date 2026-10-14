<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied:

    Line 1 – Removed: <%@ Page Title="" Language="C#" MasterPageFile="~/Patient/PatientMaster.Master"
                               AutoEventWireup="true" CodeBehind="PatientNotifications.aspx.cs"
                               Inherits="DBProject.PatientNotifications" %>
             (Web Forms Page directive not available in ASP.NET Core)

    All Web Forms server controls have been replaced:
      - <%@ Page %> directive with MasterPageFile, AutoEventWireup, CodeBehind, Inherits
                                                   → @page / @model directives in PatientNotifications.cshtml
      - asp:Content ID="Content1" (head section)   → @section head { } in PatientNotifications.cshtml
      - asp:Content ID="Content2" (body section)   → @RenderBody() content in PatientNotifications.cshtml
      - asp:Label ID="Notify"                       → @Model.Notify rendered in PatientNotifications.cshtml
      - asp:Label ID="NDoctor"                      → @Model.NDoctor rendered in PatientNotifications.cshtml
      - asp:Label ID="NTimings"                     → @Model.NTimings rendered in PatientNotifications.cshtml

    Migration target:
      Patient/PatientNotifications.cshtml       (ASP.NET Core Razor Pages view)
      Patient/PatientNotifications.cshtml.cs    (ASP.NET Core Razor Pages PageModel)

    This file is retained as a migration record only and is not compiled or served.
--%>
