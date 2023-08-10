// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using BlazorMovies.Shared.EDM;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace BlazorMovies.Server.Areas.Identity.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;

        public RegisterModel(
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            SignInManager<ApplicationUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [BindProperty]
        public InputModel Input { get; set; }

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
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            /// <summary>
            /// Custom property for User first name. It is Required.
            /// </summary>
            /// <remarks>
            /// Initializing string types to empty strings prevents null
            /// reference exceptions. 
            /// </remarks>
            [Required]
            [DataType(DataType.Text)]
            [Display(Name = "First Name")]
            public string FirstName { get; set; } = string.Empty;

            /// <summary>
            /// Custom property for User date of birth.
            /// <para>
            /// Property nullable to prevent the system from assigning a
            /// 01/01-0001 value. Nullable DateTime value defaults to
            /// DateTime.Today.
            /// </para>
            /// </summary>
            [DataType(DataType.Date)]
            [Display(Name = "Birth Date")]
            public DateTime? DateOfBirth { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Required]
            [StringLength(100, 
                ErrorMessage = "The {0} must be at least {2} and " +
                               "at max {1} characters long.", 
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", 
                ErrorMessage = 
                    "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager
                .GetExternalAuthenticationSchemesAsync())
                .ToList();
        }

        /// <summary>
        /// The form with id="registerForm" in the Register.cshtml view has a
        /// "post" method. This means that this <see cref="OnPostAsync"/>
        /// action (handler) is called when the form is submitted by clicking
        /// its "Register" button element. 
        /// </summary>
        /// <param name="returnUrl">The Url that the User is trying to access 
        /// before authentication; i.e., before sign in. It is used to redirect
        /// the User after a successful authentication. </param>
        /// <returns>An asynchronous operation with the result of the action
        /// method.</returns>
        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl ??= Url.Content("~/");

            ExternalLogins = (await _signInManager
                    .GetExternalAuthenticationSchemesAsync())
                .ToList();

            if (ModelState.IsValid)
            {
                ApplicationUser user = CreateUser();

                await _userStore
                    .SetUserNameAsync(
                        user,
                        Input.Email,
                        CancellationToken.None);

                await _emailStore
                    .SetEmailAsync(
                        user,
                        Input.Email,
                        CancellationToken.None);

                /// Custom property extends IdentityUser.
                user.FirstName = Input.FirstName;

                /// Custom property extends IdentityUser.
                user.DateOfBirth = Input.DateOfBirth;

                /// Represents the result of an identity operation; i.e., the
                /// result of creating the specified User in the backing store.
                IdentityResult result = await _userManager
                    .CreateAsync(user, Input.Password);

                if (result.Succeeded)
                {
                    _logger
                        .LogInformation(
                            "User created a new account with password.");

                    string userId = await _userManager.GetUserIdAsync(user);

                    string code = await _userManager
                        .GenerateEmailConfirmationTokenAsync(user);

                    code = WebEncoders
                        .Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                    string callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new
                        {
                            area = "Identity",
                            userId = userId,
                            code = code,
                            returnUrl = returnUrl
                        },
                        protocol: Request.Scheme);

                    string htmlEmailBody =
                        $"<p style=\"font-family:arial;\">" +
                        $"Dear {user.FirstName},</p>" +
                        $"<p style=\"font-family:arial;\">" +
                        $"Thank your for your interest in FlixManager.</p>" +
                        $"<p style=\"font-family:arial;\">" +
                        $"Please confirm your new account by clicking " +
                        $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>" +
                        $"here</a>.</p>";

                    await _emailSender
                        .SendEmailAsync(
                            Input.Email,
                            "Email confirmation request",
                            htmlEmailBody);

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation",
                            new { email = Input.Email, returnUrl = returnUrl });
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect(returnUrl);
                }
                
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }

        /// <summary>
        /// Creates an instance of type <see cref="ApplicationUser"/>.
        /// </summary>
        /// <returns>The created application user.</returns>
        /// <exception cref="InvalidOperationException"></exception>
        private ApplicationUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has" +
                    $" a parameterless constructor, or alternatively override the register " +
                    $"page in /Areas/Identity/Pages/Account/Register.cshtml");
            }
        }

        private IUserEmailStore<ApplicationUser> GetEmailStore()
        {
            if (!_userManager.SupportsUserEmail)
            {
                throw new NotSupportedException("The default UI requires a user store with email support.");
            }
            return (IUserEmailStore<ApplicationUser>)_userStore;
        }
    }
}


