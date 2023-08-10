using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Client.Pages
{
    /// <summary>
    /// Takes the <see cref="Action"/> segment of the route parameter (e.g.,
    /// login, logout) to set the appropriate callback URL. It uses JavaScript
    /// Interop to perform these operations. A JS script is required in the
    /// Application/Client/wwwroot/Index.html file.
    /// <para>
    /// The page produced by the <see cref="Authentication"/> component defines
    /// the routes required for handling different authentication states, sets
    /// UI content for authentication states, and manages authentication state.
    /// </para>
    /// <para>
    /// Authentication actions, such as registering or signing in a User are
    /// passed to the
    /// <see cref="RemoteAuthenticatorViewCore&lt;TAuthenticationState&gt;"/>
    /// component which persists and controls state across authentication
    /// operations. 
    /// </para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/?view=aspnetcore-6.0#authentication-component">
    /// Authentication component</see>.
    /// <para>
    /// <see cref="LogOutSucceeded"/> redirects the User to the
    /// Application/Client/Pages/Index after a successful logout. 
    /// </para>
    /// </summary>
    /// <remarks>
    /// The <see cref="RemoteAuthenticatorView"/> component manages performing
    /// the appropriate actions at each stage of authentication.
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0">
    /// Secure a hosted ASP.Net Core Blazor WebAssembly app with
    /// IdentityServer</see> and
    /// <see href="https://www.udemy.com/share/102l0i3@W_N3FERpad3RJJPGFzIrYF4k7_hKSRVIfxzyorMHIBsl3HRUroo80Ama8hClFYFY/">
    /// Episode 99-100</see> of Udemy course "Programming in Blazor - ASP.Net Core 5"
    /// by Felipe Gavilan.
    /// </para>
    /// </remarks>
    public partial class Authentication
    {
        [Parameter] public string? Action { get; set; }
    }
}


