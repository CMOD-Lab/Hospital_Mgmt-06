@page "/Admin/AddStaff"
@model DBProject.Admin.AddStaffModel
@{
    ViewData["Title"] = "Staff Registration";
    Layout = "~/Admin/_AdminLayout.cshtml";
}

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">

<head>
    <title>@ViewData["Title"]</title>

<style type="text/css">
    html
    {
      background-image: url("/assets/staff9.jpg");
      background-size: cover;
      background-position: 0 -90px;
      background-attachment: fixed;
    }
    body {
        background: none !important;
    }
    .backstretch {
        display: none !important;
    }

    .grad {
        background: linear-gradient(to right, rgba(30, 160, 100 , 0.15), rgba(150, 148, 255 , 1));
        border-radius: 8px;
    }

    #space {
        padding-bottom: 50px;
    }
</style>
</head>
<body>
    <div id="myclass">

        <link rel="stylesheet" href="http://fonts.googleapis.com/css?family=Roboto:400,100,300,500"/>
        <link rel="stylesheet" href="/assets/bootstrap/css/bootstrap.min.css"/>
        <link rel="stylesheet" href="/assets/font-awesome/css/font-awesome.min.css"/>
        <link rel="stylesheet" href="/assets/css/form-elements.css"/>
        <link rel="stylesheet" href="/assets/css/style.css"/>

        <!-- Top content -->
        <div class="top-content">
            <div class="inner-bg">
                <div class="container grad">
                    <div class="row">
                        <div class="col-sm-8 col-sm-offset-2 text">
                            <h1><strong>Medix4 Health Care</strong> Staff Registration Form</h1>
                            <div class="description">
                                <p>
                                    This is a <strong>"registeration form"</strong> for Medix4 Health Care.
                                    Fill out the information to register the staff member
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>

        <!-- Staff Registration Form -->
        <div class="container myclass">
            <div class="row">
                <div class="col-sm-2"></div>
                <div class="col-sm-8">
                    <div class="form-box" id="spaces">
                        <div class="form-top">
                            <div class="form-top-left">
                                <h3 style="font-family:Algerian">Staff Registration Form</h3>

                                @if (Model.SuccessMessage != null)
                                {
                                    <div class="alert alert-success" role="alert">
                                        <strong>@Model.SuccessMessage</strong>
                                    </div>
                                }
                                @if (Model.ErrorMessage != null)
                                {
                                    <div class="alert alert-danger" role="alert">
                                        @Model.ErrorMessage
                                    </div>
                                }
                            </div>
                        </div>
                        <div class="form-bottom">
                            <form method="post">
                                @Html.AntiForgeryToken()

                                <!-- Name -->
                                <div class="form-group">
                                    <span asp-validation-for="Input.Name" class="text-danger"></span>
                                    <input asp-for="Input.Name" type="text" class="form-username form-control" placeholder="Name" />
                                </div>

                                <!-- Birth Date -->
                                <div class="form-group">
                                    <span asp-validation-for="Input.BirthDate" class="text-danger"></span>
                                    <input asp-for="Input.BirthDate" type="text" class="form-username form-control" placeholder="Birth Date (mm/dd/yyyy)" />
                                </div>

                                <!-- Phone -->
                                <div class="form-group">
                                    <span asp-validation-for="Input.Phone" class="text-danger"></span>
                                    <input asp-for="Input.Phone" type="text" class="form-username form-control" placeholder="Phone Number" />
                                </div>

                                <!-- Salary, Qualification, Designation -->
                                <span asp-validation-for="Input.Salary" class="text-danger"></span>
                                <span asp-validation-for="Input.Designation" class="text-danger"></span>

                                <div class="form-group">
                                    <input asp-for="Input.Salary" type="text" placeholder="Salary in Rupees" style="width:221px" />
                                    <input asp-for="Input.Qual" type="text" placeholder="Qualification" />
                                    <input asp-for="Input.Designation" type="text" placeholder="Designation" style="width:243px" />
                                </div>

                                <!-- Address -->
                                <div class="form-group">
                                    <input asp-for="Input.Address" class="form-username form-control" type="text" placeholder="Address" />
                                </div>

                                <!-- Gender -->
                                <div class="form-group">
                                    <input type="radio" name="Input.Gender" id="Male" value="M" checked="checked" /> <label for="Male">Male</label>
                                    <input type="radio" name="Input.Gender" id="Female" value="F" /> <label for="Female">Female</label>
                                </div>

                                <button type="submit" class="btn btn-primary">Add</button>
                            </form>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>

    <!-- Footer -->
    <footer>
        <div class="container">
            <div class="row">
                <div class="col-sm-8 col-sm-offset-2">
                    <div class="footer-border"></div>
                    <p style="color:darkslategrey">if You have Any Query
                        Please Feel Free to Contact US. <i class="fa fa-smile-o"></i></p>
                </div>
            </div>
        </div>
    </footer>

    <!-- Javascript -->
    <script src="/assets/js/jquery-1.11.1.min.js"></script>
    <script src="/assets/bootstrap/js/bootstrap.min.js"></script>
    <script src="/assets/js/jquery.backstretch.min.js"></script>
    <script src="/assets/js/scripts.js"></script>

</body>
</html>
