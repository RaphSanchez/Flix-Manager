using BlazorMovies.Client;
using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Events;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Client.Helpers.ServiceExtensions;
using BlazorMovies.Shared.AuthZHelpers;

using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#region Your custom services

/// <summary>
/// This is your own configuration of custom services for the
/// application's service collection. You are effectively configuring
/// objects (classes or interfaces) as services in the dependency
/// injection (DI) system of the Client project in a Blazor WebAssembly
/// application.
/// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-6.0#register-groups-of-services-with-extension-methods
/// https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60?view=aspnetcore-6.0&tabs=visual-studio#new-hosting-model
/// https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60-samples?view=aspnetcore-6.0
/// </summary>

/// Singleton lifetime configuration creates a single instance of the
/// service (class) and all components requesting the service receive
/// the same instance.
builder.Services.AddSingleton<SingletonService>();

/// Transient lifetime configuration creates a different instance of
/// the service (class) and each component requesting the service receives
/// a new instance.
builder.Services.AddTransient<TransientService>();

/// Registers an interface as injectable. It must define the
/// interface and it must also specify the object (or class) that
/// you want to pass as a service. It can be any class that has
/// previously implemented  the IRepository interface.
builder.Services.AddTransient<IRepository, RepositoryInMemory>();

/// Configures your custom
/// Application/Client/ApiServices/ApiManager/IApiConnector service responsible
/// of encapsulating the resource methods (produce Http requests/responses) and
/// generating the URIs that target the appropriate endpoint (controller) for
/// the  required resource (data entity).
/// 
/// ApiConnector class uses an instance of HttpClient. HttpClient
/// is intended to be instantiated once and re-used throughout the
/// life of an application. Instantiating an HttpClient class for
/// every request will exhaust the number of sockets available under
/// heavy loads. The IApiConnector service must be configured with
/// a "scoped" lifetime to prevent exhausting available sockets.
/// https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient?view=net-6.0
builder.Services.AddScoped<IApiConnector, ApiConnector>();

/// Configures your custom
/// Application/Client/ApiServices/ApiManager/IApiService that exposes
/// IEntityName interfaces to this Application/Client project to send/receive
/// Http requests/responses.
/// 
/// The IApiService employs an IApiConnector service; i.e.,
/// a concrete instance of the IApiConnector must be created when
/// a component consumes the IApiService to make an Http request.
builder.Services.AddScoped<IApiService, ApiService>();

/// Application state container defines an event responsible for initiating an
/// asyncrhonous operation to update the number of pending create, update, or
/// delete operations stored in our custom IndexedDB that need to be
/// synchronized with the server.
/// 
/// Episode 152. Comunicación entre componentes - Borrado en modo offline
/// of Udemy course Programando en Blazor - ASP.Net Core 7 by Felipe Gavilán.
/// https://www.udemy.com/share/101ZK23@P_z_otCvbswCJciag5NxSmY35j18kowbLOCX8HqFrUZqiNaxg_M3YeBqkKGKTY0v/
///
/// In-memory state container service
/// https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-7.0&pivots=webassembly#in-memory-state-container-service-wasm
builder.Services.AddSingleton<ISynchronizationState, SynchronizationState>();

/// Encapsulates custom methods to handle exceptions with clear and meaningful
/// messages to inform the end user. It allows to centralize custom messages;
/// e.g., messages conveyed to the user when a JSException is thrown because
/// the user attempts a create, update, or delete operation when the application
/// is offline.
builder.Services.AddTransient<IExceptionHandlers, ExceptionHandlers>();

#endregion

#region AuthN & AuthZ

/// The HttpClient.BaseAddress property gets or sets the base
/// address or URI of the internet resource used when sending
/// requests; i.e., it takes the URL and returns a base address
/// of type URI.
///
/// The URL value is defined in the
/// Application/Server/Properties/launchsettings.json file. This
/// value is converted to a type URI by the
/// HttpClient.BaseAddress property and this is the property value
/// consumed by the Application/Repository/ApiManager/ApiConnector
/// class to build URI endpoints including any required parameters.
///
/// This block of code is replaced with the typed HttpClient service
/// below.
//builder.Services.AddScoped(sp => new HttpClient
//{
//    BaseAddress =
//    new Uri(builder.HostEnvironment.BaseAddress)
//});

/// Adds the IHttpClientFactory and related services to the IServiceCollection
/// and configures a typed HttpClient (HttpClientWithJwt) to supply HttpClient
/// instances that include authorization JWTs when making requests to the
/// Application/Server-API resources.
///
/// A call to the AddHttpMessageHandler is mandatory because it is the one
/// responsible for attaching the JWTs to outgoing HttpResponseMessage
/// instances (requests). The BaseAddressAuthorizationMessageHandler is
/// preconfigured with the app's base address as an authorized URI. 
///
/// Install NuGet package: Microsoft.Extensions.Http
/// 
/// Udemy course: "Programming in Blazor - ASP.Net Core 5" by Felipe Gavilan
/// episode 102 and Udemy course: "Complete Guide to ASP.Net Core RESTful API
/// with Blazor WASM" by Frank Liu episode 128 at 16:00 and 129.
/// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#attach-tokens-to-outgoing-requests
/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.httpclientfactoryservicecollectionextensions.addhttpclient?view=dotnet-plat-ext-6.0
builder.Services.AddHttpClient<HttpClientWithJwt>(client =>
    {
        /// The HttpClient.BaseAddress property gets or sets the base
        /// address or URI of the internet resource used when sending
        /// requests; i.e., it takes the URL and returns a base address
        /// of type URI.
        ///
        /// The URL value is defined in the
        /// Application/Server/Properties/launchsettings.json file. This
        /// value is converted to a type URI by the
        /// HttpClient.BaseAddress property and this is the property value
        /// consumed by the Application/Client/ApiManager/ApiConnector
        /// class to build URI endpoints including any required parameters.
        client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
    })
    /// Built-in service handles attaching the authorization JWTs in the
    /// request header of outgoing requests everytime we consume the
    /// HttpClientWithJwt custom class to build a request.
    /// 
    /// The Application/Client/ApiManager/ApiConnector class is responsible
    /// for building the Http requests; the ApiConnector class consumes this
    /// typed HttpClientWithJwt whenever it needs to include the authorization
    /// tokens; i.e., when it needs to access protected Application/Server-Api
    /// resources.
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

/// Adds the IHttpClientFactory and related services to the IServiceCollection
/// and configures a typed HttpCLient (HttpClientNoJwt) to supply HttpClient
/// instances that do NOT include authorization JWTs when making requests to
/// the Application/Server-Api resources.
///
/// Install NuGet package: Microsoft.Extensions.Http
/// 
/// Udemy course: "Programming in Blazor - ASP.Net Core 5" by Felipe Gavilan
/// episode 102 and Udemy course: "Complete Guide to ASP.Net Core RESTful API
/// with Blazor WASM" by Frank Liu episode 128 at 16:00 and 129.
/// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/additional-scenarios?view=aspnetcore-6.0#attach-tokens-to-outgoing-requests
/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.httpclientfactoryservicecollectionextensions.addhttpclient?view=dotnet-plat-ext-6.0
builder.Services.AddHttpClient<HttpClientNoJwt>(client =>
{
    /// The HttpClient.BaseAddress property gets or sets the base
    /// address or URI of the internet resource used when sending
    /// requests; i.e., it takes the URL and returns a base address
    /// of type URI.
    ///
    /// The URL value is defined in the
    /// Application/Server/Properties/launchsettings.json file. This
    /// value is converted to a type URI by the
    /// HttpClient.BaseAddress property and this is the property value
    /// consumed by the Application/Client/ApiManager/ApiConnector
    /// class to build URI endpoints including any required parameters.
    client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress);
});

/// Sets up the services required by the app to interact with the existing
/// authorization system to authenticate users. 
///
/// By default, configuration for the app is loaded by
/// convention from _configuration/ClientId. By convention, the client ID is
/// set to the app's assembly name. This URL can be changed to point to a
/// separate endpoint by calling the overload with options.
/// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#api-authorization-support
builder.Services
    .AddApiAuthorization();

/// Adds authorization services to the specified IService collection.
/// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/?view=aspnetcore-6.0#authentication
/// https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/
builder.Services
    .AddAuthorizationCore(options =>
    {
        /// Encapsulates the code logic to add the authorization policies
        /// registered in your custom <see cref="AuthZPolicies"/> class.
        options.AddAuthZPolicies();
    });

#endregion

/// The Application/Client/Helpers/ServiceExtensions LocalizationServices 
/// class extends the IServiceCollection to implement localization by
/// dynamically setting the culture by user preference stored using the
/// browser's local storage.
///
/// This line of code is replaced with a call to the
/// ConfigureLocalizationServices(builder) method below.
//await builder.Build().RunAsync();

/// Configures the localization services. It is an extension method that
/// allows to reduce the code logic required here in the dependency injection
/// container.
///
/// It has a call to the WebAssembly.HostBuilder.Build method and to the
/// WebAssemblyHost.RunAsync method. For this reason IT MUST BE CALLED AT
/// THE END OF THE CLASS; i.e., once all the required application services
/// have been registered.  
await builder.Services.ConfigureLocalizationServices(builder);

