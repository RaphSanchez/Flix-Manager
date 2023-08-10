using System.Globalization;

using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Provides the application user with options that represent the cultures
    /// supported by the application for localization services.
    /// </summary>
    /// <remarks>
    /// It uses JSInterop to invoke JS functions imported from JS modules in
    /// the 'local-storage.js' file to get the selected culture from the
    /// browser's local storage and to set the culture option selected by the
    /// user into the browser's local storage.
    /// <para>
    /// See Episode "138. Manually Changing the App's Language" of Udemy course
    /// <see href="https://www.udemy.com/share/102l0i3@DhvnVDpIfM5CidiZG_MZKhBe6brf9r3Ilb6P5v2y9nbI61xsVlv3eSEfINBYL2OV/">
    /// Programming in Blazor - ASP.Net Core 5</see> by Felipe Gavilán and
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-6.0">
    /// ASP.Net Core Blazor globalization and localization</see>.
    /// </para>
    /// </remarks>
    public partial class CultureSelector : IDisposable
    {
        /// <summary>
        /// The cultures supported by the application. There is a specific
        /// resource file (Application/Client/Shared/Resources) with the
        /// translated strings for each supported locale. 
        /// </summary>
        private CultureInfo[] _supportedCultures = new[]
        {
            new CultureInfo("en-US"),
            new CultureInfo("es-MX"),
        };

        /// <summary>
        /// Performs synchronous JS Interop in Blazor WebAssembly apps; i.e.,
        /// use it only in front-end projects. Synchronous JS Interop is
        /// required because CSharp properties are synchronous by nature and
        /// the <see cref="Culture"/> property requires invocation of a JS
        /// module with JS functions.
        /// </summary>
        /// <remarks>
        /// JS interop calls are asynchronous by default, regardless of whether
        /// the called code is synchronous or asynchronous. When working with
        /// <see cref="IJSObjectReference"/> to import a module that defines
        /// JS functions, you can use <see cref="IJSInProcessObjectReference"/>
        /// to make a synchronous call from .Net (front-end).
        /// <para>
        /// See <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-6.0#synchronous-js-interop-in-blazor-webassembly-apps">
        /// Synchronous JS interop in Blazor WebAssembly apps,
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-6.0#javascript-isolation-in-javascript-modules">
        /// JavaScript isolation in JavaScript modules</see>, and
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-6.0&pivots=webassembly#dynamically-set-the-culture-by-user-preference">
        /// Dynamically set the culture by user preference</see>.
        /// </see>
        /// </para>
        /// </remarks>
        private IJSInProcessObjectReference? _module;

        /// <summary>
        /// Represents a culture option that can be selected by the application
        /// user to store in the browser's local storage and use it to localize
        /// the application.
        /// </summary>
        /// <remarks>
        /// CSharp properties are synchronous by nature. We need to use
        /// <see cref="IJSInProcessObjectReference"/> to import a JS module and
        /// make synchronous invocations for JS interop calls.
        /// See <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet?view=aspnetcore-6.0#synchronous-js-interop-in-blazor-webassembly-apps">
        /// Synchronous JS interop in Blazor WebAssembly apps</see> for more
        /// info. 
        /// </remarks>
        private CultureInfo Culture
        {
            get => CultureInfo.CurrentCulture;

            set
            {

                if (CultureInfo.CurrentCulture != value)
                {
                    /// Invokes the specified JS function synchronously. 
                    _module?.InvokeVoid(
                        "setInLocalStorage", "culture", value.Name);

                    /// Forces reloading the routable component from the server
                    /// after the updated culture selection has been set. 
                    NavManager.NavigateTo(NavManager.Uri, forceLoad: true);
                }
            }
        }

        protected override async Task OnInitializedAsync()
        {
            /// Uses an instance of the injected IJSRuntime service (JsRuntime)
            /// to invoke a JS object that references the JS file with the
            /// module that defines the JS functions required.
            ///
            /// By convention, "import" is a special identifier used
            /// specifically for importing the JS module specified in the path
            /// passed as an argument.
            _module = await JsRuntime.InvokeAsync<IJSInProcessObjectReference>(
                "import", "./js/local-storage.js");
        }

        /// <summary>
        /// <see cref="IJSInProcessObjectReference"/> implements
        /// <see cref="IDisposable"/> interface.
        /// </summary>
        public void Dispose()
        {
            _module?.Dispose();
        }
    }
}


