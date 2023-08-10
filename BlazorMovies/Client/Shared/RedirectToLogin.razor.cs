using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Manages redirecting unauthorized users to the login page and preserves
    /// the current URL that the user is attempting to access so that they can
    /// be returned to that page if authentication is successful.
    /// </summary>
    /// <remarks>
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0">
    /// RedirectToLogin component</see>.
    /// </para>
    /// </remarks>
    public partial class RedirectToLogin
    {
        /// <summary>
        /// Support for authentication in Blazor WebAssembly apps changed to
        /// rely on navigation history state instead of query strings in the
        /// URL. This is the new redirection approach for apps that target
        /// .Net 7 or later.
        /// </summary>
        /// <remarks>
        /// See <see href="https://learn.microsoft.com/en-us/aspnet/core/migration/60-70?view=aspnetcore-7.0">
        /// Blazor WebAssembly authentication uses history state for redirects
        /// </see>,
        /// <see href="https://github.com/aspnet/Announcements/issues/497">
        /// [Breaking change]: Updated to Authentication in web assembly
        /// applications
        /// </see>,
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/routing?view=aspnetcore-7.0#navigation-history-state">
        /// navigation history state
        /// </see>, and
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.components.webassembly.authentication.navigationmanagerextensions.navigatetologin">
        /// NavigateToLogin
        /// </see>.
        /// </remarks>
        protected override void OnInitialized()
        {
            Navigation.NavigateToLogin(
                Options.Get(
                    Microsoft.Extensions.Options.Options.DefaultName)
                    .AuthenticationPaths.LogInPath);
        }


        /// <summary>
        /// Valid with ASP.Net 6.
        /// </summary>
        //protected override void OnInitialized()
        //{
        //    Navigation
        //        .NavigateTo($"authentication/login?returnUrl={Uri.EscapeDataString(Navigation.Uri)}");
        //}
    }
}


