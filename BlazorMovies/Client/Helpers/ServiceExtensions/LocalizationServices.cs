using System.Globalization;

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Helpers.ServiceExtensions
{
    /// <summary>
    /// The <see cref="LocalizationServices"/> class extends the
    /// <see cref="IServiceCollection"/> of the dependency injection container
    /// in the Application/Client Program class. It contains an extension
    /// method <see cref="ConfigureLocalizationServices"/> designed to register
    /// custom Localization services.
    /// </summary>
    /// <remarks>
    /// This approach permits a cleaner Application/Client Program class
    /// because most of the code logic is defined within the extension methods.
    /// The <see cref="ConfigureLocalizationServices"/> extension method
    /// defined here is called from the dependency injection container during
    /// application build up.
    /// See <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-6.0">
    /// Dynamically set the culture by user preference
    /// </see> and Episode "138. Manually Changing the App's Language" of the
    /// Udemy course <see href="https://www.udemy.com/share/102l0i3@9zdRiplt8fLNhAd_hrj2mPUn2wsIMAtYITmBRTsHyqbuC7Ggeg6v9RjCJOLmSY5K/">
    /// Programming in Blazor - ASP.Net Core 5
    /// </see> by Felipe Gavilán.
    /// </remarks>
    public static class LocalizationServices
    {
        /// <summary>
        /// The host object for Blazor running under WebAssembly.
        /// </summary>
        private static WebAssemblyHost? _host;

        /// <summary>
        /// Information about the specific culture.
        /// </summary>
        private static CultureInfo? _currentCulture;

        private static string _storedCulture = string.Empty;

        /// <summary>
        /// Represents an instance of the JS runtime to which calls may
        /// be dispatched.
        /// </summary>
        private static IJSRuntime? _js;

        /// <summary>
        /// Stores a reference to the module's external JS file. This JS object
        /// (or file) contains the JS module with the function(s) of interese;
        /// i.e., the <see cref="IJSObjectReference"/> is the type in Blazor
        /// that represents a JS module.
        /// </summary>
        /// <remarks>
        /// JS Isolation is mandatory. Otherwise, the application build up
        /// process sometimes throws an exception because the JS functions are
        /// not found. Error likely caused because JS file (e.g., utilities.js)
        /// not loaded on time.
        /// </remarks>
        private static IJSObjectReference _module = null!;

        /// <summary>
        /// Configures the localization services in the application and uses
        /// JS Interop to invoke a JS module with functions responsible for
        /// retrieving the user's preference for the current culture stored
        /// in the browser's local storage and for setting the value when the
        /// user selects a different culture..
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> that
        /// represents a collection of service descriptors.</param>
        /// <param name="builder">A builder for configuring and creating a
        /// WebAssembly host.</param>
        /// <returns>An async operation.</returns>
        public static async Task ConfigureLocalizationServices(
            this IServiceCollection services,
            WebAssemblyHostBuilder builder)
        {
            /// Adds services required for application localization.
            /// https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-6.0&pivots=webassembly#localization-1
            services.AddLocalization();

            /// Builds a WebAssenblyHost instance based on the configuration
            /// of this builder. 
            _host = builder.Build();

            /// Gets the IJSRuntime service from the IServiceProvider.
            _js = _host.Services.GetRequiredService<IJSRuntime>();

            /// Uses an instance of the injected IJSRuntime service (_js) to
            /// invoke a JS object that references the JS file with the module
            /// that defines the JS functions required.
            ///
            /// By convention, "import" is a special identifier used
            /// specifically for importing the JS module specified in the
            /// path passed as an argument.
            ///
            /// IJSObjectReference implements IDisposable interface but static
            /// classes/methods are not allowed to implment the interface. The
            /// alternative is to use an asynchronous "using" block of code.
            await using (_module = await _js.InvokeAsync<IJSObjectReference>(
                             "import", "./js/local-storage.js"))
            {
                /// Instance of an IJSObjectReference that provides a
                /// reference to the module's external JS file. It allows to
                /// invoke the specified JS function.
                ///
                /// JS function 'getFromLocalStorage' retrieves the culture
                /// value, if any, stored in the web browser's local storage.
                _storedCulture = await _module.InvokeAsync<string>(
                    "getFromLocalStorage", "culture");

                if (!string.IsNullOrEmpty(_storedCulture))
                {
                    _currentCulture = new CultureInfo(_storedCulture);
                }
                else
                {
                    _currentCulture = new CultureInfo("en-US");

                    /// Instance of an IJSObjectReference that provides a
                    /// reference to the module's external JS file. It allows to
                    /// invoke the specified JS function.
                    ///
                    /// JS function 'setInLocalStorage' persists the culture
                    /// key-value pair into the web browser's local storage.
                    await _module.InvokeVoidAsync(
                        "setInLocalStorage", "culture", "en-US");
                }
            }

            /// Sets the default localization culture for the application.
            /// ALWAYS set both values to the same culture in order to use
            /// IStringLocalizer and IStringLocalizer<T>.
            CultureInfo.DefaultThreadCurrentCulture = _currentCulture;
            CultureInfo.DefaultThreadCurrentUICulture = _currentCulture;

            /// Runs the application associated with this host
            /// (WebAssemblyHost).
            await _host.RunAsync();
        }
    }
}


