// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using BlazorMovies.Shared.EDM;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

/// Add, download, and delete custom user data to Identity in an
/// ASP.Net Core project.
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data?view=aspnetcore-6.0&tabs=visual-studio
namespace BlazorMovies.Server.Areas.Identity.Pages.Account.Manage
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [TempData]
        public string StatusMessage { get; set; }

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
        public class InputModel
        {
            /// <summary>
            ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
            ///     directly from your code. This API may change or be removed in future releases.
            /// </summary>
            [Phone]
            [Display(Name = "Phone number")]
            public string PhoneNumber { get; set; }

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
            public string FirstName { get; set; }

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
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            string userName = await _userManager
                    .GetUserNameAsync(user);

            string phoneNumber = await _userManager
                    .GetPhoneNumberAsync(user);

            Username = userName;

            Input = new InputModel
            {
                PhoneNumber = phoneNumber,

                /// Custom property extends IdentityUser.
                FirstName = user.FirstName,

                /// Custom property extends IdentityUser.
                DateOfBirth = user.DateOfBirth,
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ApplicationUser user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return 
                    NotFound($"Unable to load user with ID " +
                             $"'{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            string phoneNumber = await _userManager.GetPhoneNumberAsync(user);
            if (Input.PhoneNumber != phoneNumber)
            {
                IdentityResult setPhoneResult = await _userManager
                        .SetPhoneNumberAsync(user, Input.PhoneNumber);

                if (!setPhoneResult.Succeeded)
                {
                    StatusMessage = 
                        "Unexpected error when trying to set phone number.";
                    return RedirectToPage();
                }

                /// Updates custom User property.
                if (Input.FirstName != user.FirstName)
                {
                    user.FirstName = Input.FirstName;
                }
                /// Updates custom User property.
                if (Input.DateOfBirth != user.DateOfBirth)
                {
                    user.DateOfBirth = Input.DateOfBirth;
                }

                /// Updates the specified User in the backing store with the
                /// new property values.
                await _userManager.UpdateAsync(user);
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}

