// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BlazorMovies.Shared.EDM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;

namespace BlazorMovies.Server.Areas.Identity.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }
        public async Task<IActionResult> OnGetAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            ApplicationUser user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));

            IdentityResult result = await _userManager
                    .ConfirmEmailAsync(user, code);

            /// Built-in code replaced with code below.
            //StatusMessage = result.Succeeded
            //    ? "Thank you for confirming your email."
            //    : "Error confirming your email.";

            /// Custom code is more informational when email confirmation fails
            /// because the email confirmation token is not valid (e.g., when
            /// it has expired). The ToString method is overridden in the
            /// IdentityResult type.
            ///
            /// Requires the "Error" word because it is used by the
            /// Identity/Pages/Account/_StatusMessage to allocate the appropriate
            /// CSS class for the StatusMessage rendered.
            StatusMessage = result.Succeeded
            ? $"Thank you for confirming your email."
            : $"Error. Email Confirmation {result}";

            return Page();
        }
    }
}

