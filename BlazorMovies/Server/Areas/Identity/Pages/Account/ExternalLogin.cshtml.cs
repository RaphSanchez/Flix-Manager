// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;
using BlazorMovies.Shared.EDM;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Server.Areas.Identity.Pages.Account
{
    /// <summary>
    /// Handles trusted external identity provider authentication; e.g.,
    /// Google or Facebook authentication. 
    /// </summary>
    [AllowAnonymous]
    public class ExternalLoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUserStore<ApplicationUser> _userStore;
        private readonly IUserEmailStore<ApplicationUser> _emailStore;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ExternalLoginModel> _logger;

        public ExternalLoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            IUserStore<ApplicationUser> userStore,
            ILogger<ExternalLoginModel> logger,
            IEmailSender emailSender)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = GetEmailStore();
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
        public string ProviderDisplayName { get; set; }

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
        /// Flag informs if the User has not proved ownership of the email
        /// account provided for the local account; i.e., if email confirmation
        /// is still pending. 
        /// </summary>
        /// <remarks>
        /// True if the <see cref="ApplicationUser"/> store has a record that
        /// matches the <see cref="ClaimTypes.Email"/> claim sent back by the
        /// external identity provider after a successful authentication and
        /// its <see cref="ApplicationUser.EmailConfirmed"/> property value
        /// is false.
        /// </remarks>
        public bool PendingEmailConfirmation { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string ErrorMessage { get; set; }

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
            public string Email { get; set; }

            /// <summary>
            /// Password for local account is optional and it is used as a
            /// backup in case the external login provider is not available
            /// at some point in time. 
            /// </summary>
            [StringLength(100,
                ErrorMessage =
                    "The {0} must be at least {2} and max {1} characters long.",
                MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password (optional)")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password",
                ErrorMessage =
                    "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }
        }

        public IActionResult OnGet() => RedirectToPage("./Login");

        /// <summary>
        /// This <see cref="OnPost(string, string)"/> action is called when
        /// the form with id="external-account" in the
        /// Areas.Identity.Pages.Account.Login view is submitted.
        /// </summary>
        /// <param name="provider">The name of the button element in the 
        /// external account form of the Login.cshtml view. Its value is
        /// automatically bound to this parameter because the names match
        /// and ASP.Net Core handles the binding.</param>
        /// <param name="returnUrl">The Url that the User is trying to access 
        /// before authentication; i.e., before sign in. It is used to redirect
        /// the User after a successful authentication. </param>
        /// <returns>The result of the action method.</returns>
        public IActionResult OnPost(string provider, string returnUrl = null)
        {
            // Request a redirect to the external login provider.
            /// The Url where the external login provider redirects after a
            /// successful authentication; i.e., after successful authentication
            /// the User is redirected to the ./ExternalLogin relative path.
            /// 
            /// The external login info is redirected to ./ExternalLogin and
            /// the handler of that info is the "Callback" action: 
            /// <see cref="OnGetCallbackAsync(string, string)"/>.
            /// 
            /// The Url.Page method generates the full Url. It uses the returnUrl
            /// parameter and appends the relative path passed as an argument 
            /// (./ExternalLogin). 
            string redirectUrl = Url.Page(
                "./ExternalLogin",
                pageHandler: "Callback",
                values: new { returnUrl });

            AuthenticationProperties properties =
                _signInManager
                .ConfigureExternalAuthenticationProperties(provider, redirectUrl);

            return new ChallengeResult(provider, properties);
        }

        /// <summary>
        /// Handles the <see cref="ExternalLoginInfo"/> sent in the callback 
        /// configured in the <see cref="OnPost(string, string)"/> action. If 
        /// authentication with external provider successful, it signs in the User.
        /// </summary>
        /// <param name="returnUrl">The Url that the User is trying to access 
        /// before authentication; i.e., before sign in. It is used to redirect
        /// the User after a successful authentication. </param>
        /// <param name="remoteError">Error received from the external login 
        /// provider.
        /// </param>
        /// <returns>An asynchronous operation with the result of the action
        /// method.</returns>
        public async Task<IActionResult> OnGetCallbackAsync(string returnUrl = null,
            string remoteError = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (remoteError != null)
            {
                ErrorMessage = $"Error from external provider: {remoteError}";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            /// Represents the login information; e.g.,
            /// <see cref="ClaimsPrincipal"/> with the user claims such as
            /// name, givenname, emailaddress, etc.; a collection of
            /// <see cref="AuthenticationToken"/>, the
            /// <see cref="AuthenticationProperties"/> dictionary with state
            /// value about the authentication session, the 
            /// <see cref="UserLoginInfo.LoginProvider"/>, 
            /// <see cref="UserLoginInfo.ProviderDisplayName"/>, and the
            /// <see cref="UserLoginInfo.ProviderKey"/> used by the external
            /// provider to identify this particular User.
            ExternalLoginInfo info =
                await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ErrorMessage = "Error loading external login information.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            /// The display name for the identity provider; e.g., Google.
            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;

            #region Account not confirmed error message

            /// Episode 112 of YouTube course:
            /// "ASP.Net Core tutorial for beginners" by Kudvenkat.
            /// https://youtu.be/4XugKqgwGnU

            /// Email address sent back after a successful authentication with
            /// the external identity provider. 
            string externalEmail = info.Principal
                    .HasClaim(c => c.Type == ClaimTypes.Email)
                ? info.Principal.FindFirstValue(ClaimTypes.Email)
                : null;

            Input = new InputModel()
            {
                /// The email sent by the external identity provider is
                /// passed to the ExternalLogin view (page) to facilitate
                /// the User the creation of a local account. 
                Email = externalEmail
            };

            ApplicationUser dbUser = await _signInManager.UserManager
                .FindByEmailAsync(externalEmail);

            /// If dbUser is not null and its ApplicationUser.EmailConfirmed
            /// property value is set to false.
            if (dbUser is { EmailConfirmed: false })
            {
                PendingEmailConfirmation = true;

                /// Return the ExternalLogin.cshtml view. The page has a
                /// discrminator to determine the message conveyed to the user.
                return Page();
            }

            #endregion

            /// Employs the <see cref="SignInManager{TUser}"/> to login the
            /// User to a local User account using the
            /// <see cref="ExternalLoginInfo"/>. 
            /// 
            /// Signin cookie is set to "isPersistent: false" after the browser
            /// is closed. 
            /// 
            /// The ExternalLoginSignInAsync method employs the
            /// dbo.AspNetUserLogins[Data] table where UserId is the local
            /// UserId assigned in dbo.AspNetUsers[Data] table. If the User
            /// does not have a local User account, the else block sets in.
            Microsoft.AspNetCore.Identity.SignInResult result =
                await _signInManager
                        .ExternalLoginSignInAsync(
                            info.LoginProvider,
                            info.ProviderKey,
                            isPersistent: false,
                            bypassTwoFactor: true);

            if (result.Succeeded)
            {
                _logger
                    .LogInformation("{Name} logged in with " +
                                    "{LoginProvider} provider.",
                    info.Principal.Identity.Name, info.LoginProvider);
                return LocalRedirect(returnUrl);
            }

            if (result.IsLockedOut)
            {
                return RedirectToPage("./Lockout");
            }

            /// If SignInResult did not succeed and is not locked out, the User
            /// does not have a local account; i.e., if the User has not
            /// provided a valid email address for the local account, then ask
            /// the User to register (create) a local account.
            else
            {


                PendingEmailConfirmation = false;

                /// Return the ExternalLogin.cshtml view. The page has a
                /// discrminator to determine the message conveyed to the user
                /// and a <form> with an asp-page-handler="Confirmation" that
                /// invokes the OnPostConfirmationAsync handler (method) below.
                return Page();
            }
        }

        /// <summary>
        /// Handles the "Confirmation" form in the ExternalLogin view to
        /// register a new User with a local account.
        /// </summary>
        /// <param name="returnUrl">The Url that the User is trying to access 
        /// before authentication; i.e., before sign in. It is used to redirect
        /// the User after a successful authentication. </param>
        /// <returns>An asynchronous operation that represents the result of
        /// the action.</returns>
        public async Task<IActionResult> OnPostConfirmationAsync(
            string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            /// Represents the login information; e.g., <see cref="ClaimsPrincipal"/>
            /// with the user claims such as name, givenname, emailaddress,
            /// etc.; a collection of <see cref="AuthenticationToken"/>, the
            /// <see cref="AuthenticationProperties"/> dictionary with state value
            /// about the authentication session, the 
            /// <see cref="UserLoginInfo.LoginProvider"/>, 
            /// <see cref="UserLoginInfo.ProviderDisplayName"/>, and the
            /// <see cref="UserLoginInfo.ProviderKey"/> used by the external provider
            /// to identify this particular User.
            ExternalLoginInfo info =
                await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                ErrorMessage = "Error loading external login information during confirmation.";
                return RedirectToPage("./Login", new { ReturnUrl = returnUrl });
            }

            if (ModelState.IsValid)
            {
                ApplicationUser user = CreateUser();

                await _userStore
                    .SetUserNameAsync(user, Input.Email, CancellationToken.None);

                await _emailStore
                    .SetEmailAsync(user, Input.Email, CancellationToken.None);

                /// Represents the result of an identity operation; i.e., the
                /// result of creating the specified User in the backing store.
                /// Evaluates if User provides a backup password to use for
                /// login with local account in case external login provider is
                /// not available. 
                IdentityResult result = !string.IsNullOrEmpty(Input.Password)
                    ? await _userManager.CreateAsync(user, Input.Password)
                    : await _userManager.CreateAsync(user);

                if (result.Succeeded)
                {
                    /// Persists the user's first name into the database table.
                    user.FirstName = info.Principal.Claims
                        .FirstOrDefault(c =>
                            c.Type == ClaimTypes.GivenName)?.Value;

                    /// Adds the external <see cref="UserLoginInfo"/> to the
                    /// specified User. 
                    result = await _userManager.AddLoginAsync(user, info);

                    if (result.Succeeded)
                    {
                        _logger.LogInformation(
                            "User created an account using {Name} provider.",
                            info.LoginProvider);

                        string userId = await _userManager
                            .GetUserIdAsync(user);

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
                                code = code
                            },
                            protocol: Request.Scheme);

                        await _emailSender
                            .SendEmailAsync(
                                Input.Email,
                                "Confirm your email",
                                $"Please confirm your account by " +
                                $"<a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>" +
                                $"clicking here</a>.");

                        // If account confirmation is required, we need to
                        // show the link if we don't have a real email sender
                        if (_userManager.Options.SignIn.RequireConfirmedAccount)
                        {
                            return RedirectToPage("./RegisterConfirmation", new { Email = Input.Email });
                        }

                        await _signInManager
                            .SignInAsync(
                                user,
                                isPersistent: false,
                                info.LoginProvider);

                        return LocalRedirect(returnUrl);
                    }
                }
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            ProviderDisplayName = info.ProviderDisplayName;
            ReturnUrl = returnUrl;
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
                /// Creates an instance of the type designated by the 
                /// specified generic type parameter. 
                return Activator.CreateInstance<ApplicationUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(ApplicationUser)}'. " +
                    $"Ensure that '{nameof(ApplicationUser)}' is not an abstract class and has a " +
                    $"parameterless constructor, or alternatively override the external " +
                    $"login page in /Areas/Identity/Pages/Account/ExternalLogin.cshtml");
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

