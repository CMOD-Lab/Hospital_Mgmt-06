<%--
  Migrated from ASP.NET Web Forms (SignUp.aspx) to ASP.NET Core Razor Pages
  to support cloud-native deployment on AWS.

  Rule cr-dotnet-0026 remediation applied:
    - Removed <%@ Page %> directive with AutoEventWireup, CodeBehind, Inherits (line 1)
    - Removed <form id="SignUpPage" runat="server"> Web Forms server-side form
    - Removed asp:TextBox, asp:Button Web Forms server controls
    - Removed <%=sName.ClientID %> style server-side ID expressions in JavaScript
    - Removed Request.Form["Gender"] reference in JavaScript
    - Replaced with ASP.NET Core Razor Pages view: SignUp.cshtml
    - Replaced with ASP.NET Core Razor Pages PageModel: SignUp.cshtml.cs

  Migration target:
    SignUp.cshtml       (ASP.NET Core Razor Pages view)
    SignUp.cshtml.cs    (ASP.NET Core Razor Pages PageModel)

  This file is retained as a migration record only and is not compiled or served.
--%>
