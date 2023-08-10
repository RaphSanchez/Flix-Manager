using Microsoft.AspNetCore.Identity;

namespace BlazorMovies.Shared.EDM
{
    /// <summary>
    /// ASP.Net Core Identity model customization. The
    /// <see cref="ApplicationUser"/> class extends IdentityUser with custom
    /// properties. 
    /// </summary>
    /// <remarks>
    /// Properties decorated with the <see cref="PersonalDataAttribute"/> are
    /// deleted when the Application/Server Areas/Identity/Pages/Account/Manage
    /// DeletePersonalData.cshtml Razor page calls
    /// <see cref="UserManager{TUser}"/>.Delete and included in the downloaded
    /// data by the Application/Server Areas/Identity/Pages/Account/Manage
    /// DownloadPersonalData.cshtml Razor page. 
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-6.0#customize-the-model">
    /// Identity model customization in ASP.Net Core</see>,
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/add-user-data?view=aspnetcore-6.0">
    /// Add, download, and delete custom user data to Identity</see>, and
    /// Episode 77.
    /// <see href="https://youtube.com/playlist?list=PL6n9fhu94yhVkdrusLaQsfERmL_Jh4XmU">
    /// Extend IdentityUser in ASP.Net Core</see> of YouTube course ASP.Net
    /// Core tutorial for beginners by Kudvenkat.
    /// </para>
    /// </remarks>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// The first name of the user is different from the
        /// <see cref="ApplicationUser"/>.UserName which is used to login to
        /// the application and is set with the user email.
        /// </summary>
        [PersonalData]
        public string? FirstName { get; set; }

        /// <summary>
        /// The date of birth of the user. 
        /// </summary>
        /// <remarks>
        /// Property nullable to prevent the system from assigning a 01/01-0001
        /// value. Nullable DateTime value defaults to DateTime.Today.
        /// <para>
        /// See <see href="https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.rangeattribute?view=net-6.0">
        /// RangeAttribute class
        /// </see> and
        /// <see href="https://stackoverflow.com/questions/8844747/restrict-datetime-value-with-data-annotations">
        /// Restrict DateTime value with data annotations</see>.
        /// </para>
        /// </remarks>
        [PersonalData]
        public DateTime? DateOfBirth { get; set; }
    }
}

