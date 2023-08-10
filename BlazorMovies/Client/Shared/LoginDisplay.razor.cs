
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// This component is rendered in the Application/Client/Shared/MainLayout
    /// component.
    /// </summary>
    /// <remarks>
    /// For authenticated users, it displays the current user name, offers a
    /// link to the user profile page in ASP.Net Core Identity, and offers a
    /// button to log out of the app.
    /// <para>
    /// For anonymous users, it offers the option to register and offers the
    /// option to log in. 
    /// </para>
    /// </remarks>
    public partial class LoginDisplay
    {
        /// <summary>
        /// Support for authentication in Blazor WebAssembly apps changed in
        /// .Net 7 or later. As part of this change, the
        /// "SignOutSessionStateManager" is obsolete and replaced with
        /// <c>NavigationManager.NavigateToLogout</c>. 
        /// </summary>
        /// <remarks>
        /// The <c>SignOutSessionStateManager</c> service registration is
        /// removed in Application/Server-Api/ Program.cs.
        /// <para>
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
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.components.webassembly.authentication.navigationmanagerextensions.navigatetologout?view=aspnetcore-7.0">
        /// NavigateToLogout
        /// </see>.
        /// </para>
        /// </remarks>
        public void BeginLogOut()
        {
            Navigation.NavigateToLogout("authentication/logout");
        }

        /// <summary>
        /// Signs out a User.
        /// </summary>
        /// <remarks>
        /// Valid with ASP.Net 6.
        /// </remarks>
        //private async Task BeginSignOut(MouseEventArgs args)
        //{
        //    await SignOutManager.SetSignOutState();
        //    navManager.NavigateTo("authentication/logout");
        //}
    }
}


