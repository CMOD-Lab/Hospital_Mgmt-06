using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DBProject.DAL;

namespace DBProject.Pages.Admin
{
    // Migrated from ASP.NET Web Forms (DoctorRegistrationForm.aspx.cs) to ASP.NET Core Razor Pages
    // Replaced: System.Web.UI and System.Web.UI.WebControls -> Microsoft.AspNetCore.Mvc and Microsoft.AspNetCore.Mvc.RazorPages
    // Replaced: System.Web.UI.Page base class -> PageModel (ASP.NET Core Razor Pages)
    // Replaced: Page_Load event handler -> OnGet() method (Razor Pages lifecycle)
    // Replaced: server-side ASP.NET Web Forms controls -> model-bound properties with data annotations
    // Replaced: ServerValidateEventArgs -> server-side validation in OnPost() handler
    public class DoctorRegistrationFormModel : PageModel
    {
        [BindProperty]
        [Required(ErrorMessage = "* Required")]
        public string Name { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "*Required")]
        [RegularExpression(@"((?:0[1-9])|(?:1[0-2]))\/((?:0[0-9])|(?:[1-2][0-9])|(?:3[0-1]))\/(\d{4})", ErrorMessage = "Birth Date Format Not Correct")]
        public string BirthDate { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "*Required")]
        [EmailAddress(ErrorMessage = "Incorrect Email Format")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "*Required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords Do not Match")]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
        public string Phone { get; set; } = string.Empty;

        [BindProperty]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
        public string Salary { get; set; } = string.Empty;

        [BindProperty]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
        public string ChargesPerVisit { get; set; } = string.Empty;

        [BindProperty]
        [Range(0, 5, ErrorMessage = "Experience Range should be (0-5)")]
        public int Exp { get; set; }

        [BindProperty]
        public int Department { get; set; }

        [BindProperty]
        public string Qualification { get; set; } = string.Empty;

        [BindProperty]
        public string Specialization { get; set; } = string.Empty;

        [BindProperty]
        public string Address { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;
        public bool IsMessageVisible { get; set; } = false;
        public string EmailError { get; set; } = string.Empty;
        public string DepartmentError { get; set; } = string.Empty;

        public void OnGet()
        {
            // Initial page load - no action required
        }

        public IActionResult OnPost()
        {
            // Validate email uniqueness (replaces ValidateDoctorEmail ServerValidate)
            myDAL objmyDAL = new myDAL();
            if (objmyDAL.DoctorEmailAlreadyExist(Email) == 1)
            {
                EmailError = "This Email Already exist , kindly choose a different one !";
                ModelState.AddModelError("Email", EmailError);
            }

            // Validate department selection (replaces DepartmentValidate ServerValidate)
            if (Department == 0)
            {
                DepartmentError = "Please Select Department";
                ModelState.AddModelError("Department", DepartmentError);
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            string gender = Request.Form["Gender"].ToString();

            int exp = Exp;
            int salary = Convert.ToInt32(Salary);
            int chargesPerVisit = Convert.ToInt32(ChargesPerVisit);

            objmyDAL.AddDoctor(Name, Email, Password, BirthDate, Department, Phone,
                gender.Length > 0 ? gender[0] : 'M', Address, exp, salary, chargesPerVisit,
                Specialization, Qualification);

            IsMessageVisible = true;
            Message = "doctor Added Succesfully";
            FlushInformation();

            return Page();
        }

        private void FlushInformation()
        {
            Name = string.Empty;
            Email = string.Empty;
            Password = string.Empty;
            BirthDate = string.Empty;
            Department = 0;
            Phone = string.Empty;
            Address = string.Empty;
            Exp = 0;
            Salary = string.Empty;
            ChargesPerVisit = string.Empty;
            Specialization = string.Empty;
            Qualification = string.Empty;
        }
    }
}
