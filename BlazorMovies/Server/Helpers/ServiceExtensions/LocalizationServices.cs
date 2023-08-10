using System.Globalization;

using BlazorMovies.Shared.EDM;

using Microsoft.AspNetCore.Localization;

namespace BlazorMovies.Server.Helpers.ServiceExtensions
{
    /// <summary>
    /// The <see cref="LocalizationServices"/> class extends the
    /// IServiceCollection and the WebApplication of the dependency injection
    /// container in the Application/Server-Api Program class. It contains
    /// an <see cref="EnableLocalizationServices"/> extension method designed
    /// to add the services required for application localization and an
    /// a <see cref="ConfigureRequestLocalizationPipeline"/> extension method
    /// that adds the RequestLocalizationMiddleware to set culture information
    /// for requests based on information provided by the client. 
    /// </summary>
    /// <remarks>
    /// The <see cref="LocalizationServices"/> class is responsible
    /// for configuring localization services for the ASP.Net Core Identity
    /// UI which uses Razor Pages to handle <see cref="ApplicationUser"/>
    /// authentication and authorization operations 
    /// <para>
    /// This approach permits a cleaner Application/Server-Api Program class
    /// because most of the code logic is defined within the extension methods.
    /// </para>
    /// </remarks>
    public static class LocalizationServices
    {
        /// <summary>
        /// Represents the
        /// <see cref="RequestLocalizationOptions.DefaultRequestCulture"/> set
        /// for the application. 
        /// </summary>
        private const string EnUsCulture = "en-US";

        /// <summary>
        /// Enables the Microsoft.Extensions.Localization.IStringLocalizer and
        /// the Microsoft.Extensions.Localization.IStringLocalizerFactory.
        /// For more info visit the Localization section in
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-6.0"/>
        /// and <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-6.0#configure-localization">
        /// Configure Localization</see>.
        /// </summary>
        /// <remarks>
        /// Globalization and localization are configured in the
        /// Application/Server-Api Program class to enable localization of the
        /// ASP.Net Core Identity UI which uses Razor Pages to handle
        /// ApplicationUser authentication and/or authorization operations.
        /// <para>
        /// Globalization and localization are configured in the
        /// Application/Client Program class to enable localization of the
        /// Application/Client UI.
        /// </para>
        /// </remarks>
        public static void EnableLocalizationServices(
            this IServiceCollection services)
        {
            /// https://learn.microsoft.com/en-us/dotnet/core/extensions/localization#register-localization-services
            services.AddLocalization(options =>
            {
                options.ResourcesPath = "Resources";
            });
        }

        /// <summary>
        /// Adds the RequestLocalizationMiddleware to set culture information
        /// for requests based on information provided by the client. See
        /// YouTube video <see href="https://youtu.be/WGYvThTvwCY">
        /// Localizing ASP.Net 6.0 Razor Pages
        /// </see> by Medhat Elmasry, 
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/select-language-culture?view=aspnetcore-6.0#use-a-custom-provider">
        /// Use a custom provider
        /// </see>, and <see href="https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/select-language-culture?view=aspnetcore-6.0#configure-localization-middleware">
        /// Localization middleware</see>.
        /// </summary>
        /// <remarks>
        /// On every request, the list of RequestCultureProviders in the
        /// RequestLocalizationOptions is enumerated and the first provider
        /// that can successfully determine the request culture is used.
        /// </remarks>
        public static void ConfigureRequestLocalizationPipeline(
            this WebApplication app)
        {
            /// Adds the RequestLocalizationMiddleware to automatically set
            /// culture information for requests based on requests based on
            /// the user's preference.
            app.UseRequestLocalization(options =>
            {
                CultureInfo[] supportedCultures = new[]
                {
                    new CultureInfo(EnUsCulture),
                    new CultureInfo("es-MX")
                };

                options.DefaultRequestCulture =
                    new RequestCulture(culture: EnUsCulture);

                /// Sets the default localization culture for the application.
                /// ALWAYS set both values to the same culture in order to use
                /// IStringLocalizer and IStringLocalizer<T>.
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;

                /// Modifies the default order in which the request culture
                /// providers are applied; i.e., removes the
                /// AcceptLanguageHeaderRequestCulture provider from the last
                /// indexed position and inserts it at the first indexed
                /// position followed by QueryStringRequestCultureProvider and
                /// the CookieRequestCultureProvider.
                /// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-6.0#change-request-culture-providers-order
                IRequestCultureProvider languageHeaderCultureProvider =
                    options.RequestCultureProviders[2];

                options.RequestCultureProviders
                    .RemoveAt(2);

                options.RequestCultureProviders
                    .Insert(0, languageHeaderCultureProvider);
            });
        }
    }
}


