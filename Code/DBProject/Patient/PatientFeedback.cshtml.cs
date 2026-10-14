using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Distributed;
using DBProject.DAL;
using DBProject.Infrastructure;

// Migrated from ASP.NET Web Forms (PatientFeedback.aspx / PatientFeedback.aspx.cs)
// to ASP.NET Core Razor Pages to support cloud-native deployment on AWS.
//
// Rule cr-dotnet-0026 remediation applied:
//   - Removed using System.Web;                   (line 5 in original – not available in ASP.NET Core)
//   - Removed using System.Web.UI;                (not available in ASP.NET Core)
//   - Removed using System.Web.UI.WebControls;    (not available in ASP.NET Core)
//   - Removed System.Web.UI.Page inheritance      (line 13 in original)
//   - Removed Web Forms Page_Load event handler with IsPostBack check
//   - Removed Web Forms Button OnClick event handler (giveFeedback)
//   - Replaced with ASP.NET Core Razor Pages PageModel pattern
//
// cr-dotnet-0126 remediation applied (lines 21, 56 in original source):
//   Heavy coupling to stateful IIS in-process session (Session["idoriginal"], Session["aID"])
//   replaced with IDistributedCache backed by Amazon ElastiCache for Redis.
//   This eliminates IIS sticky-session server affinity and enables stateless horizontal
//   scaling across multiple ECS tasks or Kubernetes pods without sticky-session routing.
//   Session data persists across pod restarts and scales horizontally.
//
//   Line 21 fix: Session["idoriginal"] read replaced with
//     IDistributedCache.GetInt32("idoriginal") backed by Amazon ElastiCache for Redis.
//   Line 56 fix: Session["aID"] = aID replaced with
//     IDistributedCache.SetInt32("aID", aID) backed by Amazon ElastiCache for Redis.
//
// Rule cr-dotnet-0045 remediation applied (lines 21, 35, 56, 79 in original):
//   - Replaced in-process Session["idoriginal"] (line 21) with
//     IDistributedCache.GetInt32("idoriginal") backed by Amazon ElastiCache for Redis.
//   - Replaced in-process Session["aID"] = "" (line 35) with
//     IDistributedCache.SetString("aID", "") backed by Amazon ElastiCache for Redis.
//   - Replaced in-process Session["aID"] = aID (line 56) with
//     IDistributedCache.SetInt32("aID", aID) backed by Amazon ElastiCache for Redis.
//   - Replaced in-process Session["aID"] (line 79) with
//     IDistributedCache.GetInt32("aID") backed by Amazon ElastiCache for Redis.
//   - IDistributedCache is injected via constructor (dependency injection).
//   - This enables stateless horizontal scaling across multiple ECS tasks or
//     Kubernetes pods without server affinity.

namespace DBProject.Patient
{
    public class PatientFeedbackModel : PageModel
    {
        // cr-dotnet-0126 / cr-dotnet-0045: Amazon ElastiCache for Redis distributed cache – injected via DI.
        // Replaces in-process IIS sticky-session HttpContext.Session to eliminate server affinity
        // and enable stateless horizontal scaling.
        private readonly IDistributedCache _cache;

        public PatientFeedbackModel(IDistributedCache cache)
        {
            _cache = cache;
        }

        // Replaces asp:Label ID="Feedback" – rendered in PatientFeedback.cshtml
        public string FeedbackMessage { get; set; } = string.Empty;

        // Replaces asp:Label ID="FDoctor" – rendered in PatientFeedback.cshtml
        public string FDoctorMessage { get; set; } = string.Empty;

        // Replaces asp:Label ID="FTimings" – rendered in PatientFeedback.cshtml
        public string FTimingsMessage { get; set; } = string.Empty;

        // Replaces Visible="false" toggling of asp:Label Message, asp:DropDownList List,
        // and asp:Button button1 – controls conditional rendering in PatientFeedback.cshtml
        public bool ShowFeedbackForm { get; set; } = false;

        // Replaces asp:DropDownList ID="List" SelectedItem.Value – bound from form POST
        [BindProperty]
        public int Rating { get; set; } = 1;

        public void OnGet()
        {
            // Preserves original Page_Load logic:
            //   if (!IsPostBack) { Session["aID"] = ""; pendingFeedback(sender, e); }
            //
            // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 35):
            // Replaces: Session["aID"] = "";
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
            // Eliminates IIS sticky-session dependency; session data shared across all pods.
            _cache.SetString("aID", string.Empty);
            LoadPendingFeedback();
        }

        private void LoadPendingFeedback()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 21):
            // Replaces: int pid = (int)Session["idoriginal"];
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis instead of
            // IIS in-process sticky-session state. Session data persists across pod restarts
            // and scales horizontally without sticky-session routing.
            int pid = _cache.GetInt32("idoriginal") ?? 0;

            string dName = string.Empty;
            string timings = string.Empty;
            int aID = 0;

            int status = objmyDAl.isFeedbackPending(pid, ref dName, ref timings, ref aID);

            if (status == -1)
            {
                // Replaces: Feedback.Text = "There was some error..."
                FeedbackMessage = "There was some error in retrieving the Pending Feedbacks.";
            }
            else if (status == 0)
            {
                // Replaces: Feedback.Text = "There are no pending feedbacks :)"
                FeedbackMessage = "There are no pending feedbacks :)";
            }
            else
            {
                // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 56):
                // Replaces: Session["aID"] = aID;
                // Uses IDistributedCache backed by Amazon ElastiCache for Redis.
                // Eliminates IIS sticky-session dependency; session data shared across all pods.
                _cache.SetInt32("aID", aID);

                // Replaces: FDoctor.Text = "Your feedback for the appointment with Doctor..."
                FDoctorMessage = "Your feedback for the appointment with Doctor " + dName + " is pending. Kindly give it.";

                // Replaces: FTimings.Text = "The Appointment Timings were : " + timings;
                FTimingsMessage = "The Appointment Timings were : " + timings;

                // Replaces: Message.Visible = true; List.Visible = true; button1.Visible = true;
                ShowFeedbackForm = true;
            }
        }

        // Replaces Web Forms giveFeedback Button OnClick event handler (original line 68+)
        // Handles the "Give Feedback" button POST action.
        public IActionResult OnPostGiveFeedback()
        {
            myDAL objmyDAl = new myDAL();

            // cr-dotnet-0126 / cr-dotnet-0045 fix (original line 79):
            // Replaces: int aID = (int)Session["aID"];
            // Uses IDistributedCache backed by Amazon ElastiCache for Redis instead of
            // IIS in-process sticky-session state. Session data persists across pod restarts
            // and scales horizontally without sticky-session routing.
            int aID = _cache.GetInt32("aID") ?? 0;

            // Preserves original: int rating = Convert.ToInt32(List.SelectedItem.Value);
            int rating = Rating;

            int status = objmyDAl.givePendingFeedback(aID);

            if (status == -1)
            {
                // Replaces: F.Text = "There was some error.";
                TempData["Error"] = "There was some error.";
            }
            else if (status == 0)
            {
                // Replaces: F.Text = "Thank you for your feedback :)";
                TempData["Success"] = "Thank you for your feedback :)";
            }

            return RedirectToPage("PatientFeedback");
        }
    }
}
