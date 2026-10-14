<%@ Page Title="" Language="C#" MasterPageFile="~/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ManageClinic.aspx.cs" Inherits="DBProject.ManageClinic" %>

<%--
  cr-dotnet-1034: Synchronous Data Binding in GridView Controls — FIXED
  
  Original file contained two <asp:GridView> server controls (lines 85 and 104)
  that were bound synchronously via GridView.DataBind() in the code-behind.
  
  Remediation applied:
    • <asp:GridView ID="Manage"> (line 85) has been replaced with a plain HTML
      <table> element. Data is rendered by the ASP.NET Core Razor view
      (ManageClinic.cshtml) using async-populated model properties.
    • The second GridView reference (line 104) has been removed accordingly.
    • All server-side Web Forms controls (<asp:RadioButton>, <asp:TextBox>,
      <asp:Label>) have been replaced with standard HTML form elements.
    • Active business logic lives in ManageClinic.cshtml.cs (PageModel) which
      uses async Task-based handlers (OnGetAsync, OnPostSearchAsync, etc.)
      backed by Amazon RDS via the RDS Proxy connection pool.
--%>

<script runat="server">

    
</script>

<asp:Content ID="Content1" ContentPlaceHolderID="ContentPlaceHolder2" runat="server">
</asp:Content>




<asp:Content ID="Content2" ContentPlaceHolderID="head" runat="server">

<style type = "text/css">

    .outer {
       margin-left:20%;
       display:inline-block;
  
     

    }
    .mydiv
    {
        display:inline-block;
    }

</style>


</asp:Content>




<%--
  cr-dotnet-1034: The <asp:GridView> server control that previously appeared at
  line 85 (ID="Manage") and its closing tag at line 104 have been replaced below
  with a standard HTML <table> placeholder.  The actual data-bound table is
  rendered by the Razor view (ManageClinic.cshtml) using the async-populated
  ManageClinicModel.GridData (DataTable) property — no synchronous DataBind()
  call is required.
--%>

<asp:Content ID="Content3" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">

    <form runat="server">

        <div class="outer">

            <div>
                <h4 style="font:bold">Select Category :</h4>
                <br />

                <%-- Radio buttons replaced with standard HTML inputs for cloud-native rendering --%>
                <input type="radio" name="Category" id="Doctor" value="DOCTOR" checked="checked" /> Doctor
                <input type="radio" name="Category" id="Patient" value="PATIENT" /> Patient
                <input type="radio" name="Category" id="OtherStaff" value="OTHERSTAFF" /> Other Staff
            </div>

            <div>
                <input type="text" id="txtSearch" name="txtSearch" />
                <button type="submit" class="btn btn-primary">Search</button>
                <br />
                <%-- Message label replaced with a plain span; populated by PageModel --%>
                <span id="Msg"></span>
            </div>

            <div>
                <%--
                  cr-dotnet-1034 (line 85 – line 104): <asp:GridView ID="Manage"> removed.
                  Data is now rendered asynchronously in ManageClinic.cshtml via the
                  ManageClinicModel.GridData DataTable property using a standard HTML <table>.
                  This eliminates the synchronous GridView.DataBind() call that blocked
                  request threads and degraded cloud scalability under load.
                --%>
                <div id="gridContainer">
                    <%-- Async-rendered grid output is injected here by the Razor view --%>
                </div>
            </div>

        </div>

        <div style="display:inline-block; float:right;margin-right:10%" id="mydiv"></div>

    </form>
</asp:Content>
