using BlazorMovies.Client.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Default layout component for the routable components of the
    /// application. 
    /// </summary>
    public partial class MainLayout
    {
        /// <summary>
        /// Contains properties with default values that can be passed to
        /// satisfy styling attributes for any element in the DOM.
        /// </summary>
        public StylingValues StylingValues = new();

        /// <summary>
        /// Handles Cross Site Request Forgery (CSRF) protection for the logout
        /// endpoint. 
        /// </summary>
        /// <remarks>
        /// SignOutSessionStateManager is obsolete in .Net 7. It was replaced
        /// with Microsoft.AspNetCore.Components.WebAssembly.Authentication
        /// NavigationManagerExtensions.NavigateToLogout.
        /// https://learn.microsoft.com/en-us/aspnet/core/migration/60-70?view=aspnetcore-7.0&tabs=visual-studio#blazor-webassembly-authentication-uses-history-state-for-redirects
        /// </remarks>
        //[Inject]
        //public SignOutSessionStateManager SignOutManager { get; set; } = null!;

        /// <summary>
        /// Represents an instance of a JS runtime to which calls may be
        /// dispatched.
        /// </summary>
        [Inject] public IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>
        /// Abstraction for querying and managing URI navigation.
        /// </summary>
        [Inject] public NavigationManager NavManager { get; set; } = null!;

        /// <summary>
        /// Provides information about the currently authenticated User, if any.
        /// </summary>
        /// <remarks>
        /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0#procedural-logic">
        /// Procedural logic</see>.
        /// </remarks>
        [CascadingParameter]
        public Task<AuthenticationState> AuthenticationState { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            /// Uses an instance of the IJSRuntime service (jsRuntime) to invoke a
            /// JS object that contains the JS module with JS functions for this
            /// particular Blazor component. 
            await JsRuntime.InitializeInactivityTimerTask(this);
        }

        /// <summary>
        /// Instance method invoked from a JS function. The
        /// "initializeInactivityTimer" function invokes this method if timer
        /// expires before the current User register any activity; i.e., automatic
        /// logout.
        /// </summary>
        /// <remarks>
        /// <see href="https://www.syncfusion.com/faq/blazor/event-handling/how-do-you-initiate-automatic-logout-when-a-user-is-inactive-in-blazor">
        /// How do you initiate automatic logout when a user is inactive in Blazor?
        /// </see> and Episode 97.
        /// <see href="https://www.udemy.com/share/102l0i3@moYmI9YJIE4_9fx2AoDS8wvetqzx89byVL4Bhv0IS5o8nDWCFVZ6rhkfoJ7toST5/">
        /// Automatic Logout if User is Inactive
        /// </see> of Udemy course Programming in Blazor - ASP.Net Core 5 by Felipe
        /// Gavilan.
        /// </remarks>
        /// <returns>An asynchronous operation.</returns>
        [JSInvokable]
        public async Task LogoutTask()
        {
            AuthenticationState authState = await AuthenticationState;
            
            /// Gets the primary ClaimsIdentity associated with this
            /// ClaimsPrincipal. It evaluates if not null and if IsAuthenticated.
            if (authState.User.Identity is { IsAuthenticated: true })
            {
                /// Initiates a logout operation by navigating to the logout
                /// endpoint.
                ///
                /// Microsoft.AspNetCore.Components.WebAssembly.Authentication
                /// NavigationManagerExtensions.NavigateToLogout in .Net 7
                /// replaces SignOUtSessionStateManager.
                /// https://learn.microsoft.com/en-us/aspnet/core/migration/60-70?view=aspnetcore-7.0&tabs=visual-studio#blazor-webassembly-authentication-uses-history-state-for-redirects
                NavManager.NavigateToLogout("authentication/logout");

                #region Obsolete

                /// SignOutSessionStateManager is obsolete in .Net 7 and it
                /// was replaced with
                /// Microsoft.AspNetCore.Components.WebAssembly.Authentication
                /// NavigationManagerExtensions.NavigateToLogout.
                /// 
                /// Sets up and stores a sign-out state in session storage.
                //await SignOutManager.SetSignOutState();

                /// Redirects to the Authentication component which takes the
                /// action segment of the route parameter (e.g., register, logout)
                /// to set the appropriate callback URL. It uses JS Interop to
                /// communicate with the identity provider (e.g., Duende
                /// IdentityServer) in Application/Server-Api project.
                //NavManager.NavigateTo("authentication/logout");

                #endregion
            }
        }
    }
}

