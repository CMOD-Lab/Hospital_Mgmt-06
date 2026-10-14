@page
@model DBProject.Pages.Admin.AdminHomeModel
@{
    ViewData["Title"] = "Admin Home";
    Layout = "~/Pages/Admin/_AdminLayout.cshtml";
}

<br />
<h1 style="font-family: 'Times New Roman', Times, serif;border-radius:5px; text-decoration: underline; background-color: #CCCCCC"><strong style="margin:37%">Clinic Stats</strong></h1>
<br /><br />

<div style="margin-left: 70px">

    <h4><strong>Total Number of Registered Doctors: </strong></h4>
    <span id="TotalPatients" style="font-weight:bold; font-size:medium">@Model.TotalDoctors</span>
    <br /><br />

    <h4><strong>Total Registered Patients: </strong></h4>
    <span id="Total_Doctors" style="font-weight:bold; font-size:medium">@Model.TotalPatients</span>
    <br /><br />

    <h4><strong>Total Income: </strong></h4>
    <span id="TotalIncome" style="font-weight:bold; font-size:medium">@Model.TotalIncome</span>
    <br /><br />

    <h3><strong style="margin:5%">Current Appointments</strong></h3>

    <table class="table table-bordered table-striped" style="background-color:white; border-color:#DEDFDE; color:black;">
        <thead style="background-color:#6B696B; color:white; font-weight:bold;">
            @if (Model.Appointments != null && Model.Appointments.Columns.Count > 0)
            {
                <tr>
                    @foreach (System.Data.DataColumn col in Model.Appointments.Columns)
                    {
                        <th>@col.ColumnCaption</th>
                    }
                </tr>
            }
        </thead>
        <tbody>
            @if (Model.Appointments != null)
            {
                @foreach (System.Data.DataRow row in Model.Appointments.Rows)
                {
                    <tr style="background-color:#F7F7DE;">
                        @foreach (var item in row.ItemArray)
                        {
                            <td>@item</td>
                        }
                    </tr>
                }
            }
        </tbody>
    </table>

</div>

<div style="margin:20%">

    <h2><strong style="margin:20%">Department Information</strong></h2>

    <table class="table table-bordered table-striped" style="background-color:white; border-color:#DEDFDE; color:black; height:50px;">
        <thead style="background-color:#6B696B; color:white; font-weight:bold;">
            @if (Model.Departments != null && Model.Departments.Columns.Count > 0)
            {
                <tr>
                    @foreach (System.Data.DataColumn col in Model.Departments.Columns)
                    {
                        <th>@col.ColumnCaption</th>
                    }
                </tr>
            }
        </thead>
        <tbody>
            @if (Model.Departments != null)
            {
                @foreach (System.Data.DataRow row in Model.Departments.Rows)
                {
                    <tr style="background-color:#F7F7DE;">
                        @foreach (var item in row.ItemArray)
                        {
                            <td>@item</td>
                        }
                    </tr>
                }
            }
        </tbody>
    </table>

</div>
