// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using BlazorMovies.Server.Models;
using BlazorMovies.Shared.EDM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BlazorMovies.Server.Areas.Identity.Pages.Account
{
    /// <summary>
    /// The Login form is displayed when the Login link is selected and/or
    /// when a User attempts to access a restricted page and has not been
    /// authenticated by the system.
    /// </summary>
    /// <remarks>
    /// When the form on the Login page is submitted, the
    /// <see cref="OnPostAsync"/> action is called.
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-6.0">
    /// Log in</see>.
    /// </para>
    /// </remarks>
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(SignInManager<ApplicationUser> signInManager,
            ILogger<LoginModel> logger)
        {
            _signInManager = signInManager;
            _logger = logger;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure
        ///     and is not intended to be used directly from your code. This API may
        ///     change or be removed in future releases.
        /// </summary>
        /// <remarks>
        /// The <see cref="LoginInputModel"/> class was originally named
        /// InputModel and resided within this <see cref="LoginModel"/> class
        /// but it was moved to a custom <see cref="Models"/> directory.
        /// Otherwise, the ResourceManager and/or ResourceReader were unable to
        /// provide the culture-specific resources at runtime.
        /// <para>
        /// See YouTube video <see href="https://youtu.be/WGYvThTvwCY">
        /// Localizing ASP.Net 6.0 Razor Pages</see> and
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-6.0">
        /// Globalization and localization in ASP.Net Core</see>.
        /// </para>
        /// </remarks>
        [BindProperty]
        public LoginInputModel Input { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        /// <remarks>
        /// Captures the Url that the User is trying to access before 
        /// authentication; i.e., before sign in. It is used to redirect the
        /// User after a successful authentication. 
        /// See <see href="https://youtu.be/fgzRnlB992s">
        /// Episode 106. ASP.Net Core google authentication setting up the UI
        /// </see> by Kudvenkat.
        /// </remarks>
        public string ReturnUrl { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// Called when the User navigates to the Login page or is redirected
        /// to be authenticated to get access to secured resources.
        /// </summary>
        /// <remarks>
        /// The login nav menu is specified in the
        /// Pages/Shared/_LoginPartial.cshtml file. 
        /// </remarks>
        public async Task OnGetAsync(string returnUrl = null)
        {
            if (!string.IsNullOrEmpty(ErrorMessage))
            {
                ModelState.AddModelError(string.Empty, ErrorMessage);
            }

            returnUrl ??= Url.Content("~/");

            // Clear the existing external cookie to ensure a clean login process
            await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

            ExternalLogins =
                (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();

            ReturnUrl = returnUrl;
        }

        /// <summary>
        /// Action called when the Login page is submitted.
        /// </summary>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            /// Gets a collection of type AuthenticationScheme for all the
            /// configured external login providers; e.g., Google, Facebook,
            /// etc.
            ExternalLogins = (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();

            if (ModelState.IsValid)
            {
                #region Account not confirmed error message

                /// Episode 112 of YouTube course:
                /// "ASP.Net Core tutorial for beginners" by Kudvenkat.
                /// https://youtu.be/4XugKqgwGnU

                /// Retrieves the current User from the data store.
                ApplicationUser user = await _signInManager.UserManager
                    .FindByEmailAsync(Input.Email);

                /// If user not null and its EmailConfirmed property is false.
                if (user is { EmailConfirmed: false }
                    /// Ensures provided combination of user name & password to
                    /// login is correct to prevent account enumeration attacks.
                    /// https://youtu.be/4XugKqgwGnU
                    && await _signInManager.UserManager
                        .CheckPasswordAsync(user, Input.Password))
                {
                    ModelState.AddModelError(string.Empty,
                        "Email not confirmed yet.");

                    return Page();
                }

                #endregion

                // This doesn't count login failures towards account lockout
                // To enable password failures to trigger account lockout, set
                // lockoutOnFailure: true
                SignInResult result = await _signInManager
                    .PasswordSignInAsync(
                        Input.Email, Input.Password,
                        Input.RememberMe,
                        lockoutOnFailure: true);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User logged in.");
                    return LocalRedirect(returnUrl);
                }
                if (result.RequiresTwoFactor)
                {
                    return RedirectToPage("./LoginWith2fa",
                        new
                        {
                            ReturnUrl = returnUrl,
                            RememberMe = Input.RememberMe
                        });
                }
                if (result.IsLockedOut)
                {
                    _logger.LogWarning("User account locked out.");
                    return RedirectToPage("./Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty,
                        "Invalid login attempt.");
                    return Page();
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}



