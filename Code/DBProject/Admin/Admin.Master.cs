using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

// Migrated from ASP.NET Web Forms Master Page code-behind (System.Web.UI.MasterPage)
// to ASP.NET Core Razor Pages layout support class as part of cloud-readiness
// remediation (cr-dotnet-0026).
//
// In ASP.NET Core Razor Pages the concept of a Master Page is replaced by a
// shared Razor Layout file (_AdminLayout.cshtml).  Layout files do not have a
// dedicated code-behind class; shared behaviour is instead placed in a base
// PageModel that individual page models can inherit from, or in Tag Helpers /
// View Components.
//
// The original Admin.Master.cs contained only an empty Page_Load handler, so
// no business logic needs to be preserved.  This file is retained for
// compatibility with the existing project structure and documents the migration.

namespace DBProject.Admin
{
    /// <summary>
    /// Base PageModel for Admin area pages.
    /// Replaces the Web Forms <c>System.Web.UI.MasterPage</c> code-behind.
    /// Individual Admin Razor Pages should inherit from this class when they
    /// require shared Admin-area initialisation logic.
    /// </summary>
    public class AdminLayoutModel : PageModel
    {
        /// <summary>
        /// Shared initialisation executed before each Admin page loads.
        /// Equivalent to the Web Forms MasterPage.Page_Load event handler.
        /// </summary>
        public virtual void OnGet()
        {
            // Shared Admin layout initialisation logic goes here.
            // The original Page_Load was empty; no business logic to migrate.
        }
    }
}
