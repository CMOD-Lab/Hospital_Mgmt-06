<%--
    This file has been migrated to ASP.NET Core Razor Pages.
    Rule cr-dotnet-0026 remediation applied:

    Line 1 – Removed: <%@ Page Title="" Language="C#" MasterPageFile="~/Patient/PatientMaster.Master"
                               AutoEventWireup="true" CodeBehind="PatientHome.aspx.cs"
                               Inherits="DBProject.PatientHome" %>
             (Web Forms Page directive not available in ASP.NET Core)

    All Web Forms server controls have been replaced:
      - asp:Content / ContentPlaceHolderID="head"           → @section head { } in PatientHome.cshtml
      - asp:Content / ContentPlaceHolderID="ContentPlaceHolder1" → @RenderBody() in _PatientLayout.cshtml
      - asp:Label ID="PName"      → @Model.PName      in PatientHome.cshtml
      - asp:Label ID="PPhone"     → @Model.PPhone     in PatientHome.cshtml
      - asp:Label ID="PBirthDate" → @Model.PBirthDate in PatientHome.cshtml
      - asp:Label ID="PatientAge" → @Model.PatientAge in PatientHome.cshtml
      - asp:Label ID="PGender"    → @Model.PGender    in PatientHome.cshtml
      - asp:Label ID="PAddress"   → @Model.PAddress   in PatientHome.cshtml

    Migration targets:
      Patient/PatientHome.cshtml     (ASP.NET Core Razor Pages view)
      Patient/PatientHome.cshtml.cs  (ASP.NET Core Razor Pages PageModel)
      Patient/_PatientLayout.cshtml  (ASP.NET Core Razor Pages Layout – replaces PatientMaster.Master)

    This file is retained as a migration record only and is not compiled or served.
--%>
