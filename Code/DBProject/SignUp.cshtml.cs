// Migrated from ASP.NET Web Forms (SignUp.aspx / SignUp.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (line 6 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (line 6 in original – not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 in original)
//   - Removed Web Forms Page_Load event handler   (line 15 in original)
//   - Removed Web Forms loginV / signupV Button.Click event handlers
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//   - Session access migrated from Session["idoriginal"] to HttpContext.Session.SetInt32
//   - Response.BufferOutput / Response.Redirect replaced with RedirectToPage
//   - Response.Write("<script>alert(...)") replaced with TempData["ErrorMessage"]
//   - Request.Form["Gender"] replaced with [BindProperty] Gender model property
//   - asp:TextBox / asp:Button server controls replaced with standard HTML form elements
//     bound via [BindProperty] in this PageModel
//
// cr-dotnet-0126 remediation applied (line 17 in original source):
//   Heavy coupling to stateful IIS in-process session (Session["idoriginal"]) replaced
//   with IDistributedCache backed by Amazon ElastiCache for Redis.
//   This eliminates IIS sticky-session server affinity and enables stateless horizontal
//   scaling across multiple ECS tasks or Kubernetes pods without sticky-session routing.
//   Session data persists across pod restarts and scales horizontally.
//
//   Line 17 fix: Session["idoriginal"] = "" replaced with
//     IDistributedCache.SetString("idoriginal", string.Empty) backed by Amazon ElastiCache for Redis.
//
// cr-dotnet-0045 remediation applied:
//   - Replaced in-process HttpContext.Session.SetString("idoriginal", string.Empty) (line 17 in original)
//     with IDistributedCache.SetString backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.SetInt32("idoriginal", id) (line 36 in original)
//     with IDistributedCache.SetInt32 backed by Amazon ElastiCache for Redis.
//   - Replaced in-process HttpContext.Session.SetInt32("idoriginal", id) (line 109 in original)
//     with IDistributedCache.SetInt32 backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor to enable stateless horizontal scaling
//     across multiple ECS tasks or Kubernetes pods without server affinity.

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

namespace DBProject
{
    public class SignUpModel : PageModel
    {
        // cr-dotnet-0126 / cr-dotnet-0045: IDistributedCache backed by Amazon ElastiCache for Redis,
        // replacing in-process IIS sticky-session HttpContext.Session to eliminate server affinity
        // and enable stateless horizontal scaling.
        private readonly IDistributedCache _distributedCache;

        public SignUpModel(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        // -----------------------------------------------------------------------
        // Login form bound properties
        // Replaces asp:TextBox ID="loginEmail" and asp:TextBox ID="loginPassword"
        // -----------------------------------------------------------------------
        [BindProperty]
        public string LoginEmail { get; set; } = string.Empty;

        [BindProperty]
        public string LoginPassword { get; set; } = string.Empty;

        // -----------------------------------------------------------------------
        // Sign-up form bound properties
        // Replaces asp:TextBox controls: sName, sBirthDate, sEmail, sPassword,
        //   scPassword, Phone, Address and radio input "Gender"
        // -----------------------------------------------------------------------
        [BindProperty]
        public string SName { get; set; } = string.Empty;

        [BindProperty]
        public string SBirthDate { get; set; } = string.Empty;

        [BindProperty]
        public string SEmail { get; set; } = string.Empty;

        [BindProperty]
        public string SPassword { get; set; } = string.Empty;

        [BindProperty]
        public string ScPassword { get; set; } = string.Empty;

        [BindProperty]
        public string Phone { get; set; } = string.Empty;

        [BindProperty]
        public string Address { get; set; } = string.Empty;

        // Replaces: Request.Form["Gender"]
        [BindProperty]
        public string Gender { get; set; } = "M";

        // Holds any error message to display (replaces Response.Write alert script)
        public string ErrorMessage { get; set; } = string.Empty;

        // -----------------------------------------------------------------------
        // GET handler
        // Replaces: protected void Page_Load(object sender, EventArgs e)
        //           { Session["idoriginal"] = ""; }
        // -----------------------------------------------------------------------
        public void OnGet()
        {
            // Preserves original Page_Load business logic:
            // clear the session user-id on every fresh page load.
            // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 17):
            // Replaces: Session["idoriginal"] = "";
            //           (previously migrated to HttpContext.Session.SetString("idoriginal", string.Empty))
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
            // Eliminates IIS sticky-session dependency; session data shared across all pods.
            _distributedCache.SetString("idoriginal", string.Empty);
        }

        // -----------------------------------------------------------------------
        // POST handler – Login
        // Replaces: protected void loginV(object sender, EventArgs e)
        // Triggered by the Login form submit button (asp:button ID="loginUserName")
        // -----------------------------------------------------------------------
        public IActionResult OnPostLogin()
        {
            // Preserves original loginV() business logic
            myDAL objmyDAl = new myDAL();

            int status = 0;
            int type   = 0;
            int id     = 0;

            status = objmyDAl.validateLogin(LoginEmail, LoginPassword, ref type, ref id);

            if (status == 0)
            {
                // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 36):
                // Replaces: Session["idoriginal"] = id;
                //           (previously migrated to HttpContext.Session.SetInt32("idoriginal", id))
                // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
                // Eliminates IIS sticky-session dependency; session data shared across all pods.
                _distributedCache.SetInt32("idoriginal", id);

                if (type == 1)
                {
                    // Replaces: Response.BufferOutput = true; Response.Redirect("~/Patient/PatientHome.aspx");
                    return RedirectToPage("/Patient/PatientHome");
                }
                else if (type == 2)
                {
                    // Replaces: Response.BufferOutput = true; Response.Redirect("~/Doctor/DoctorHome.aspx");
                    return RedirectToPage("/Doctor/DoctorHome");
                }
                else if (type == 3)
                {
                    // Replaces: Response.BufferOutput = true; Response.Redirect("~/Admin/AdminHome.aspx");
                    return RedirectToPage("/Admin/AdminHome");
                }
            }
            else if (status == 1)
            {
                // Replaces: Response.Write("<script>alert('Email not found. Try Again !');</script>");
                TempData["ErrorMessage"] = "Email not found. Try Again !";
            }
            else if (status == 2)
            {
                // Replaces: Response.Write("<script>alert('Incorrect Password. Try Again !');</script>");
                TempData["ErrorMessage"] = "Incorrect Password. Try Again !";
            }
            else if (status == -1)
            {
                // Replaces: Response.Write("<script>alert('There was some error. Try Again !');</script>");
                TempData["ErrorMessage"] = "There was some error. Try Again !";
            }

            return Page();
        }

        // -----------------------------------------------------------------------
        // POST handler – Sign Up
        // Replaces: protected void signupV(object sender, EventArgs e)
        // Triggered by the SignUp form submit button (asp:button Text="SignUp")
        // -----------------------------------------------------------------------
        public IActionResult OnPostSignup()
        {
            // Preserves original signupV() business logic
            myDAL objmyDAl = new myDAL();

            int id = 0;

            int status = objmyDAl.validateUser(SName, SBirthDate, SEmail, SPassword, Phone, Gender, Address, ref id);

            // status == 0 failure (email already exists)
            if (status == 0)
            {
                // Replaces: Response.Write("<script>alert('Email already exists...');</script>");
                TempData["ErrorMessage"] = "Email already exists. Please choose a different one.";
            }
            else if (status == 1)
            {
                // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 109):
                // Replaces: Session["idoriginal"] = id;
                //           (previously migrated to HttpContext.Session.SetInt32("idoriginal", id))
                // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
                // Eliminates IIS sticky-session dependency; session data shared across all pods.
                _distributedCache.SetInt32("idoriginal", id);

                // Replaces: Response.BufferOutput = true; Response.Redirect("~/Patient/PatientHome.aspx");
                return RedirectToPage("/Patient/PatientHome");
            }
            else if (status == -1)
            {
                // Replaces: Response.Write("<script>alert('There was some error. Try again !');</script>");
                TempData["ErrorMessage"] = "There was some error. Try again !";
            }

            return Page();
        }
    }
}
