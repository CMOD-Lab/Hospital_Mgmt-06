using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DBProject.DAL;

// Migrated from ASP.NET Web Forms (System.Web.UI.Page) to ASP.NET Core Razor Pages (PageModel)
// as part of cloud-readiness remediation (cr-dotnet-0026).
// The Web Forms code-behind pattern (System.Web.UI / System.Web.UI.WebControls) has been
// replaced with a stateless PageModel that follows 12-factor app principles and supports
// horizontal scaling in AWS cloud environments.

namespace DBProject.Admin
{
    public class AddStaffModel : PageModel
    {
        [BindProperty]
        public StaffInputModel Input { get; set; } = new StaffInputModel();

        public string? SuccessMessage { get; private set; }
        public string? ErrorMessage { get; private set; }

        public void OnGet()
        {
            // Page initialisation – no server-side state required
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            myDAL objmyDAL = new myDAL();

            int salary = Convert.ToInt32(Input.Salary);
            char gender = string.IsNullOrEmpty(Input.Gender) ? 'M' : Input.Gender[0];

            int result = objmyDAL.AddStaff(
                Input.Name,
                Input.BirthDate,
                Input.Phone,
                gender,
                Input.Address,
                salary,
                Input.Qual,
                Input.Designation);

            if (result == 1)
            {
                SuccessMessage = Input.Designation + " Added Successfully";
                FlushInformation();
            }
            else
            {
                ErrorMessage = "An error occurred while adding the staff member. Please try again.";
            }

            return Page();
        }

        private void FlushInformation()
        {
            Input = new StaffInputModel();
        }
    }

    public class StaffInputModel
    {
        [Required(ErrorMessage = "*Required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Required")]
        [RegularExpression(@"((?:0[1-9])|(?:1[0-2]))\/((?:0[0-9])|(?:[1-2][0-9])|(?:3[0-1]))\/(\d{4})",
            ErrorMessage = "Birth Date Format Not Correct")]
        public string BirthDate { get; set; } = string.Empty;

        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Required")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "Numbers Only !")]
        public string Salary { get; set; } = string.Empty;

        public string Qual { get; set; } = string.Empty;

        [Required(ErrorMessage = "*Required")]
        public string Designation { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string Gender { get; set; } = "M";
    }
}
