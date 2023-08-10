using BlazorMovies.Server.Areas.Identity.Pages.Account;

using System.ComponentModel.DataAnnotations;

namespace BlazorMovies.Server.Models
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    /// <remarks>
    /// The <see cref="LoginInputModel"/> class was originally named InputModel
    /// and resided within its <see cref="LoginModel"/> related class but it
    /// was moved outside to enable localization. Otherwise, the ResourceManager
    /// and/or ResourceReader were unable to provide culture-specific resources
    /// at runtime.
    /// <para>
    /// It was moved to a custom <see cref="Models"/> directory so it can be
    /// considered by the ResourceManager and/or the ResourceReader. The
    /// <see cref="BlazorMovies.Server.Resources"/> directory has a matching
    /// Models directory with the associated resource files that contain the
    /// locale translated text.
    /// </para>
    /// <para>
    /// The <see cref="DisplayAttribute"/> allows to specify localizable strings
    /// for types and members of entity partial classes. Its
    /// <see cref="DisplayAttribute.Name"/> property sets a value that is used
    /// for display in the UI.
    /// </para>
    /// <para>
    /// The <see cref="ValidationAttribute.ErrorMessage"/> property value is used
    /// as the "Key" in the associated resource file with the translated text. 
    /// </para>
    /// <para>
    /// See YouTube video <see href="https://youtu.be/WGYvThTvwCY">
    /// Localizing ASP.Net 6.0 Razor Pages</see> and
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-6.0">
    /// Globalization and localization in ASP.Net Core</see>.
    /// </para>
    /// </remarks>
    public class LoginInputModel
    {
        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email is not valid.")]
        [Display(Name = "Your Email")]
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [Display(Name = "Your Password")]
        public string Password { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}

