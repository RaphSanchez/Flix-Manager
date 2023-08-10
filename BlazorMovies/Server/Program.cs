using System.Text.Json.Serialization;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.DataStore;
using BlazorMovies.Server.FileStorageManager;
using BlazorMovies.Server.Helpers;
using BlazorMovies.Server.Repositories;
using BlazorMovies.Shared.EDM;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using BlazorMovies.Shared.AuthZHelpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NSwag.Generation.Processors.Security;
using NSwag;
using BlazorMovies.Server.Helpers.ServiceExtensions;
using Microsoft.AspNetCore.Components;

#pragma warning disable CS1587

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
/// For configuration info visit:
/// https://docs.microsoft.com/en-us/dotnet/architecture/blazor-for-web-forms-developers/config

/// <summary>
/// Enables the Microsoft.Extensions.Localization.IStringLocalizer<T> and the
/// Microsoft.Extensions.Localization.IStringLocalizerFactory.
/// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization?view=aspnetcore-6.0#configure-localization
/// For more info visit the Localization section in:
/// https://learn.microsoft.com/en-us/aspnet/core/blazor/globalization-localization?view=aspnetcore-6.0&pivots=webassembly#localization-1
/// </summary>
/// <remarks>
/// Globalization and localization are configured in the Application/Server-Api
/// Program class to enable localization of the ASP.Net Core Identity UI which
/// handles ApplicationUser authentication and/or authorization operations.
///
/// Globalization and localization are configured in the Application/Client
/// Program class to enable localization of the Application/Client UI.
/// </remarks>
builder.Services.EnableLocalizationServices();

#region API Rate Limiting

/// Configures the ASP.Net 7 built-in API Rate Limiting services to control
/// how much (or often) an API resource can be accessed. It is an extension
/// method that allows to reduce the code logic required here in the dependency
/// injection container.
/// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#enableratelimiting-and-disableratelimiting-attributes
builder.Services.ConfigureRateLimitingServices();

#endregion

#region Custom Services

/// https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60?view=aspnetcore-6.0&tabs=visual-studio#new-hosting-model
/// https://docs.microsoft.com/en-us/aspnet/core/migration/50-to-60-samples?view=aspnetcore-6.0

/// Retrieves the connection string from the Application/Server-Api
/// appsettings.json cofiguration source file.
string connectionString = builder.Configuration
                              .GetConnectionString("DevelopmentDb")
                          ?? throw new InvalidOperationException(
                              "Connection string 'DevelopmentDb' not found.");

/// Configures SqlServer to connect to the database using the AppDbContext
/// class that derives from DbContext. 
/// https://medium.com/executeautomation/asp-net-core-6-0-minimal-api-with-entity-framework-core-69d0c13ba9ab
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

/// <summary>
/// Configures System.Text.Json serializer-deserializer options.
/// </summary>
/// <remarks>
/// The Application/Repository/ApiManager/ApiConnector class uses
/// System.Text.Json to serialize-deserialize Http requests-responses.
/// <para>
/// This is a global configuration but it can also be configured
/// locally. For example, configuring specific resource methods of
/// the Application/Repository/ApiManager/ApiConnector class.
/// </para>
/// </remarks>
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        /// Configuration ignores an object (e.g., data entity or property)
        /// when a reference cylce is detected during serialization; i.e.,
        /// tells the serializer to set circular reference properties to
        /// null.
        /// <remarks>
        /// Circular references are a common occurrence with data entities
        /// that include "collection navigation - inverse navigation" properties.
        /// </remarks>
        /// https://stackoverflow.com/questions/60197270/jsonexception-a-possible-object-cycle-was-detected-which-is-not-supported-this
        /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-preserve-references?pivots=dotnet-6-0#ignore-circular-references
        /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-configure-options?pivots=dotnet-6-0#web-defaults-for-jsonserializeroptions
        /// https://docs.microsoft.com/en-us/learn/modules/persist-data-ef-core/3-migrations?source=learn
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;

        /// System.Text.Json looks for case-sensitive property name
        /// matches between JSON and the target object properties.
        /// However, web defaults for JsonSerializerOptions is
        /// CASE-INSENSITIVE. It was left here for illustrative purposes.
        /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-configure-options?pivots=dotnet-6-0#web-defaults-for-jsonserializeroptions
        /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-character-casing
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });

/// Configures an IUnitOfWork service with a UnitOfWork
/// implementation which has access to the AppDbContext
/// class to perfrom CRUD operations on the database.
///
/// This service allows you to inject an IUnitOfWork
/// service, wherever in your Application/Server-API
/// project, which will provide an instance of the
/// UnitOfWork class.
/// 
/// Transient lifecycle to deterministically dispose the
/// DbContext derived instance (e.g., AppDbContext) when a
/// business transaction completes. The UnitOfWork class
/// implements IDisposable and IAsyncDisposable interfaces
/// adhering to the protocol established by the IUnitOfWork
/// interface. 
/// 
/// Otherwise the DbContext instance will, by default,
/// continue tracking all the entities that go through it.
/// https://docs.microsoft.com/en-us/ef/ef6/fundamentals/working-with-dbcontext#lifetime
/// https://docs.microsoft.com/en-us/ef/core/change-tracking/
builder.Services.AddTransient<IUnitOfWork, UnitOfWork>();

/// The HttpContext encapsulates all HTTP-specific information
/// about an individual Http request such as user, authorization,
/// authentication, request, response, session, etc. Every
/// Http request creates a new object of HttpContext with current
/// information.
/// 
/// Configures the HttpContext as a service with its default
/// implementation HttpContextAccessor. The HttpContext can
/// be injected and accessed as a service through the
/// IHttpContextAccessor interface.
///
/// An instance of the HttpContext class is created on every HTTP
/// Request.
/// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-context?view=aspnetcore-6.0
/// https://www.telerik.com/blogs/how-to-get-httpcontext-asp-net-core
builder.Services.AddHttpContextAccessor();

/// Configures an IFileStorageService with an AzureStorageService
/// implementation class which enables communication to an Azure
/// storage account-container-blob.
///
/// The procedure followed is from Udemy course Programming in
/// Blazor - ASP.Net Core 5 by Felipe Galivan ep. 69 Saving an
/// image in Azure Storage.
/// 
/// The instructor registers the service with a Scoped lifetime. However,
/// "Transient" is more efficient in a Blazor web app (Application/Server).
/// https://docs.microsoft.com/en-us/dotnet/azure/sdk/dependency-injection
/// https://devblogs.microsoft.com/azure-sdk/best-practices-for-using-azure-sdk-with-asp-net-core/
builder.Services.AddTransient<IFileStorageService, AzureStorageService>();

/// Configures an IFileStorageService with an InAppStorageService
/// or an AzureStorageService implementation class which enables
/// operations to save, retrieve, update, or delete application 
/// assets (data objects) in containers that reside in the
/// applications web server root directory or in an Azure storage
/// service.
///
/// The procedure followed is from Udemy course Programming in
/// Blazor - ASP.Net Core 5 by Felipe Galivan ep. 69 Saving an
/// image in Azure Storage.
/// 
/// The instructor registers the service with a Scoped lifetime. However,
/// "Transient" is more efficient in a Blazor web app (Application/Server). 
/// https://docs.microsoft.com/en-us/dotnet/azure/sdk/dependency-injection
/// https://devblogs.microsoft.com/azure-sdk/best-practices-for-using-azure-sdk-with-asp-net-core/
//builder.Services.AddTransient<IFileStorageService, InAppStorageService>();

#endregion

#region AuthN & AuthZ

builder.Services
    /// Calling AddDefaultIdentity is equivalent to calling AddIdentity,
    /// AddDefaultUI, and AddDefaultTokenProviders which enables the
    /// default token providers to generate tokens for reset passwords,
    /// change email, and change telephone.
    /// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#startup-class
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio#adddefaultidentity-and-addidentity
    /// https://youtu.be/egITMrwMOPU
    .AddDefaultIdentity<ApplicationUser>(options =>
    {
        /// Sets a flag indicating that a confirmed account is required to
        /// sign-in. It defaults to false.
        /// 
        /// RequireConfirmedAccount is a new higher level option that if set,
        /// will use the IUserConfirmation service to determine if a User is
        /// confirmed. The default implementation of this just uses
        /// RequireConfirmedEmail. 
        /// https://github.com/dotnet/AspNetCore.Docs/issues/13206
        /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-6.0
        options.SignIn.RequireConfirmedAccount = true;

        /// Sets the token options for the Identity system.
        ///
        /// It adds the specified (Key:Value) pair =>
        /// (TokenProviderKey:Instance of conrete type for the token provider).
        /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio#change-the-email-token-lifespan
        options.Tokens.ProviderMap.Add("CustomEmailConfirmation",
            new TokenProviderDescriptor(
                typeof(CustomEmailConfirmationTokenProvider<ApplicationUser>)));

        /// Sets the token provider used to generate tokens used in account
        /// confirmation emails. The token provider is registered as a
        /// transient service in the Account Confirmation region below.
        options.Tokens.EmailConfirmationTokenProvider =
            "CustomEmailConfirmation";
    })
    /// Configures EF Core to call ASP.Net Core Identity tables; i.e.,
    /// it adds ASP.Net Core Identity Services.
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services
    /// Adds the IdentityServer engine.
    .AddIdentityServer()
    /// Sets up the default conventions on top of IdentityServer for
    /// ASP.Net Core scenarios. It provides support for authenticating
    /// users by setting up the services required by the application to
    /// interact with the existing authorization system.
    ///
    /// By default, configuration for the app is loaded from
    /// Application/Server-Api/Controllers/OidcConfigurationController.cs.
    ///
    /// By convention, the client ID is set to the app's assembly name.
    ///
    /// You can also configure the clients and resources through code using
    /// an overload of AddApiAuthorization that takes an action to
    /// configure options.
    /// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#addapiauthorization
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-6.0#oidcconfigurationcontroller
    /// https://auth0.com/docs/get-started/apis/scopes/openid-connect-scopes
    /// https://developers.onelogin.com/openid-connect/scopes
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-6.0#application-profiles
    /// 
    /// ApiAuthorizationOptions defined here are the equivalent of the
    /// configuration defined in the "Config" class of a Duende IdentityServer
    /// engine hosted in a separate ASP.Net Core web app project.
    ///
    /// You can configure IdentityResources, ApiScopes, ApiResources, and
    /// Clients. In this context, configuration is for the Clients, not for
    /// the Users. Do not include ApiResources or ApiScopes because they will
    /// be included in the Client and these are used for AuthZ to Server-Api
    /// resources.
    ///
    /// Warning!!! defining any options here overrides configuration provided
    /// through the Application/Server-Api/appsettings.json configuration
    /// resource file.
    .AddApiAuthorization<ApplicationUser, AppDbContext>()
    /// Enables your custom profile service responsible for including claims
    /// in authorization and/or authentication tokens for the current User
    /// (ClaimsPrincipal) doing the Http request. 
    .AddProfileService<IdentityProfileService>();

builder.Services
    /// AuthN service uses registered AuthN handlers to authenticate a User
    /// and to respond when an unauthenticated User tries to access a
    /// restricted resource. Registered AuthN handlers and their
    /// configuration options are called "schemes". The authentication
    /// scheme selects which handler is responsible for generating the
    /// correct set of claims. 
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/?view=aspnetcore-6.0
    .AddAuthentication()
    /// Configures a policy scheme for the application as the default
    /// authentication handler. The policy is configured to allow Identity
    /// to handle all requests routed to any subpath in the Identity URL
    /// space /Identity. The JwtBearerHandler handles all other requests.
    /// 
    /// Additionally, it registers an "ApplicationName.API" resource with
    /// IdentityServer with a default scope of "ApplicationName.API" and
    /// configures the JWT Bearer token middleware to validate tokens
    /// issued by IdentityServer engine for the app.
    /// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#addidentityserverjwt
    .AddIdentityServerJwt()
    /// Adds Google OAuth-based authentication to allow application Users to
    /// signin with their Google account.
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-6.0&tabs=visual-studio
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/google-logins?view=aspnetcore-6.0
    .AddGoogle(googleOptions =>
    {
        googleOptions.ClientId = builder.Configuration
            .GetRequiredSection("Authentication:Google:ClientId").Value;

        googleOptions.ClientSecret = builder
            .Configuration["Authentication:Google:ClientSecret"];

        /// Optional path if the User does not approve the authorization demand
        /// requested by this application.
        googleOptions.AccessDeniedPath = "/Identity/Account/AccessDenied";

    })
    /// Adds Facebook OAuth-based authentication to allow application Users to
    /// sigin with their Facebook account.
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/?view=aspnetcore-6.0&tabs=visual-studio
    /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-6.0
    .AddFacebook(facebookOptions =>
    {
        facebookOptions.AppId = builder.Configuration
            .GetRequiredSection("Authentication:Facebook:AppId").Value
                                ?? throw new InvalidOperationException();

        facebookOptions.AppSecret = builder
            .Configuration["Authentication:Facebook:AppSecret"]
                                    ?? throw new InvalidOperationException();

        /// Optional path if the User does not approve the authorization demand
        /// requested by this application.
        //facebookOptions.AccessDeniedPath = "/Identity/Account/AccessDenied";
    });

/// <summary>
/// An AuthZ policy is registered as part of the AuthZ service and can have 
/// one or more requirements. All requirements must be met for policy
/// evaluation to succeed and you can create your own custom handler with
/// a collection of data parameters to evaluate the current principal; i.e.,
/// with multiple custom requirements with custom data parameters.
/// </summary>
/// <remarks>
/// AuthZ policies ensure that the Api (or any protected resource such as a
/// Razor page, a controller, or an action method) will check for the presence
/// of a claim, with a value that matches the registered value, in the access
/// token, authorization token, presented by the Client-User trying to access
/// the resource. This is known as "Claims-based authorization".
/// <para>
/// When an "identity" is created, it may be assigned one or more "scopes" and
/// "claims" issued by a trusted party (e.g., Application/Server-Api
/// IdentityServer engine or an external identity provider like Google). A
/// claim is a value pair that represents what the subject is, not what it can
/// do. 
/// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-6.0
/// </para>
/// <para>
/// The access token will contain a given "scope" or "claim" and  its value
/// only if the Application/Client-User requests it and if the
/// Application/Server-Api IdentityServer allows the Client/User to have that
/// claim. 
/// </para>
/// <para>
/// Policies can be enforced at various levels; e.g., globally, to Razor
/// components, for all API endpoints, or for specific controllers/actions.
/// </para>
/// </remarks>
/// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-6.0
/// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/claims?view=aspnetcore-6.0
/// https://andrewlock.net/introduction-to-authorisation-in-asp-net-core/
/// https://chrissainty.com/securing-your-blazor-apps-configuring-policy-based-authorization-with-blazor/
/// https://docs.duendesoftware.com/identityserver/v6/apis/aspnetcore/authorization/
builder.Services
    .AddAuthorization(options =>
    {
        /// Encapsulates the code logic to add the authorization policies
        /// registered in your custom <see cref="AuthZPolicies"/> class.
        options.AddAuthZPolicies();
    });

/// Registers an action used to configure a particular set of options for the
/// type parameter passed; e.g., IdentityOptions.
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-6.0
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-6.0&tabs=visual-studio#configure-identity-services
/// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#configure-options-with-a-delegate
builder.Services
    .Configure<IdentityOptions>(options =>
    {
        #region Default Lockout settings

        /// The time a User is locked out for when a lockout occurs. 
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);

        /// Sets the number of failed access attempts before a user is locked
        /// out.
        options.Lockout.MaxFailedAccessAttempts = 5;

        /// Indicates if a new User can be locked out. 
        options.Lockout.AllowedForNewUsers = true;

        #endregion

        #region Default Password settings

        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequiredLength = 6;
        options.Password.RequiredUniqueChars = 1;

        #endregion

        #region Default User settings

        /// Note that these are the default allowed characters for the "user
        /// name" field, NOT for the password.
        options.User.AllowedUserNameCharacters =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

        /// Sets a flag to indicate the application requires a unique email
        /// for its users. Particularly relevant when the email is not the
        /// user name (as is the case with the default ASP.Net Core Identity
        /// templates).
        options.User.RequireUniqueEmail = true;
        #endregion
    });

#endregion

#region User Account Confirmation

/// Registers the service responsible for sending emails and logging the
/// result of the operation.
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio#configure-app-to-support-email
builder.Services
    .AddTransient<IEmailSender, EmailSenderSmtp>();

/// Registers your custom AuthMessageSenderOptions configuration instance
/// responsible for managing the AuthMessageSenderOptions type that enables
/// retrieving (sensitive information) in a development machine from the
/// user secrets.
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio#configure-app-to-support-email
/// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#configure-options-with-a-delegate
builder.Services
    .Configure<AuthMessageSenderOptions>(builder.Configuration);

/// Configures the application cookie.
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-configuration?view=aspnetcore-6.0#cookie-settings
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio#change-email-and-activity-timeout
builder.Services
    .ConfigureApplicationCookie(options =>
    {
        /// Configures confirmation email and activity timeout. The default
        /// inactivity timeout is 14 days.
        /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio#change-email-and-activity-timeout

        /// Controls how long the AuthN ticket stored in the cookie will remain
        /// valid from the point it is created. 
        options.ExpireTimeSpan = TimeSpan.FromDays(7);

        /// If true, it instructs the handler to re-issue a new cookie with a
        /// new expiration time anytime it processes a request that is more
        /// than halfway through the expiration window. 
        options.SlidingExpiration = true;
    });

/// Configures the data protection token (identity tokens) lifespans.
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio#change-all-data-protection-token-lifespans
/// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#configure-options-with-a-delegate
builder.Services
    .Configure<DataProtectionTokenProviderOptions>(options =>
    {
        /// Amount of time a generated token remains valid. The default value
        /// is 1 day. 
        options.TokenLifespan = TimeSpan.FromHours(12);
    });

/// Adds the CustomEmailConfirmationTokenProvider configured above in the AuthN
/// and AuthZ region as a transient service.
/// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0&tabs=visual-studio#change-the-email-token-lifespan
builder.Services
    .AddTransient<CustomEmailConfirmationTokenProvider<ApplicationUser>>();

#endregion

/// In combination with UseDeveloperExceptionPage (in the pipeline config
/// section), this captures database-related exceptions that can be resolved
/// by using EF migrations. When exceptions occur, ah HTML response with
/// details about possible actions to resolve the issue is generated.
/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.dependencyinjection.databasedeveloperpageexceptionfilterserviceextensions.adddatabasedeveloperpageexceptionfilter?view=aspnetcore-6.0
builder.Services
    .AddDatabaseDeveloperPageExceptionFilter();

/// <summary>
/// Configures MVC services for commonly used features with
/// controllers with "views". 
///  </summary>
/// https://docs.microsoft.com/en-us/aspnet/core/tutorials/choose-web-ui?view=aspnetcore-6.0
/// https://docs.duendesoftware.com/identityserver/v5/quickstarts/2_interactive/
builder.Services
    .AddControllersWithViews()
    /// Adds support for localized MVC view files. 
    /// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/localization/make-content-localizable?view=aspnetcore-6.0#configure-localization-services
    .AddViewLocalization()
    /// Adds support for localized DataAnnotations validation messages through
    /// IStringLocalizer abstractions.
    .AddDataAnnotationsLocalization();

/// Razor Pages authorization conventions in ASP.Net Core.
/// https://docs.microsoft.com/en-us/aspnet/core/security/authorization/razor-pages-authorization?view=aspnetcore-6.0
builder.Services.AddRazorPages();

#region NSwag

/// Adds services required for OpenApi v3.0 generation. Requires installation
/// of the NSwag.AspNetCore NuGetPackage.
/// https://github.com/RicoSuter/NSwag/wiki/AspNetCore-Middleware
/// To implement NSwag with Swagger v2.0 you can follow the ASP.Net Core
/// documentation:
/// https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-nswag?view=aspnetcore-7.0&tabs=visual-studio#add-and-configure-swagger-middleware
builder.Services.AddOpenApiDocument(options =>
{
    /// Enables manual JWT authorization token authentication.
    /// https://github.com/RicoSuter/NSwag/wiki/AspNetCore-Middleware#enable-manual-jwt-token-authentication
    /// https://www.thecodebuzz.com/nswag-jwt-token-authorization-openapi-documentation-asp-net-core/#aioseo-nswag-jwt-authorize-button-on-swagger-ui
    options.AddSecurity(
        "Bearer",
        Enumerable.Empty<string>(),
        new OpenApiSecurityScheme
        {
            Type = OpenApiSecuritySchemeType.Http,
            Scheme = JwtBearerDefaults.AuthenticationScheme,
            BearerFormat = "JWT",
            Description = "Type into the textbox: '{your JWT AuthZ token}'."

        });
    options.OperationProcessors
        .Add(new AspNetCoreOperationSecurityScopeProcessor("Bearer"));

    /// Internal identifier for the document. Defaults to "v1" but it can be
    /// customized. 
    options.DocumentName = "v1";

    /// Executes an additional process (post process) on the generated document.
    /// https://github.com/RicoSuter/NSwag/wiki/AspNetCore-Middleware#post-process-the-served-openapiswagger-specification-document
    /// https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-nswag?view=aspnetcore-7.0&tabs=visual-studio#api-info-and-description
    options.PostProcess = document =>
    {
        document.Info.Version = "v1";
        document.Info.Title = "Flix-Manager API";
        document.Info.Description = "Available API endpoints of the " +
                                    "Flix-Manager web app. Run the app, " +
                                    "login with a valid user, retrieve " +
                                    "the AuthZ (access) token from the " +
                                    "'Application' tab of the browser's " +
                                    "dev tools, and use it here to " +
                                    "'Authorize' requests.";

        document.Info.Contact = new NSwag.OpenApiContact
        {
            Name = "Rafael Sanchez",
            Email = "contacto@rafaelsanchez.ws",
            Url = "https://rafaelsanchez.ws"
        };

        /// For illustrative purposes because you have not yet defined the
        /// terms of service.
        document.Info.TermsOfService = "None";

        /// For illustrative purposes because you have not yet defined the
        /// license to grant.
        document.Info.License = new NSwag.OpenApiLicense
        {
            Name = string.Empty,
            Url = string.Empty
        };
    };
});

#endregion

#region API Caching

/// Configures the ASP.Net 7 built-in OutputCaching services to
/// programmatically define exactly how you want your cache to behave without
/// taking into consideration the HTTP cache headers.
/// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-7.0#add-the-middleware-to-the-app
///
/// It is disabled because the application was transformed to adhere to the
/// PWA standard which employs a service worker that also caches data with
/// the added benefit of optionally making it available offline.
//builder.Services.ConfigureOutputCachingServices();

#endregion

#region Web Push Notifications

/// Registers your custom VapidOptions configuration instance which can be used
/// to access sensitive information in a development machine from the application
/// user secrets. In our case, it enables access to the VAPID details for web
/// push notifications.
/// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0#bind-hierarchical-configuration
builder.Services
    .Configure<VapidOptions>(builder.Configuration);

/// Allows the Application/Server-Api to send push messages to the servers of
/// the web browser employed by the end user. The browser's push service in turn
/// transmits the message to the end user.
builder.Services
    .AddScoped<IPushNotificationsService, PushNotificationsService>();

#endregion

/// <summary>
/// Builds the WebApplication and returns a configured WebApplication.
/// </summary>
var app = builder.Build();

#region Block for IIS Server publishing

/// Runs a migration after building the application to publish with IIS server.
/// Episode 110. Deploying to IIS - ASP.Net Core Hosted  from Udemy course:
/// Programming in Blazor - ASP.Net Core 5 by Felipe Gavilan.
/// https://www.udemy.com/share/102l0i3@6u10Gs3TOhvpJnrq7t3EGe8asoJE_va6LdikaHY3ecnKtGpjfY03IVGo5EZDIw5C/ 
//using (IServiceScope scope = app.Services.CreateScope())
//{
//    IServiceProvider services = scope.ServiceProvider;
//    AppDbContext? context = services.GetService<AppDbContext>();
//    context?.Database.Migrate();
//}

#endregion


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    /// Captures exception instances from the pipeline and generates HTML
    /// responses. It requires the .AddDatabaseDeveloperPageExceptionFilter
    /// service registered above.
    app.UseDeveloperExceptionPage();

    /// Adds middleware needed for debugging Blazor WASM apps inside Chromoium
    /// dev tools. 
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

/// Adds middleware for redirecting HTTP requests to HTTPS.
/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.httpspolicybuilderextensions.usehttpsredirection?view=aspnetcore-6.0
/// https://docs.microsoft.com/en-us/aspnet/core/security/enforcing-ssl?view=aspnetcore-6.0&tabs=visual-studio#require-https
app.UseHttpsRedirection();

#region NSwag

/// Adds the OpenAPI/Swagger document generator middleware that uses the
/// ASP.Net Core API Explorer to serve openapi.json or swagger.json.
/// 
/// If not specified explicitly, the default route (relative path) for the
/// document is:
/// 
/// "/swagger/{documentName}/swagger.json"
///
/// where {documentName} defaults to 'v1' unless specified otherwise in
/// the .AddOpenApiDocument options above. In our case we explicitly
/// defined it as "v1". 
/// https://github.com/RicoSuter/NSwag/wiki/AspNetCore-Middleware
app.UseOpenApi(options =>
{
    /// Default relative path for the json document can be customized.
    options.Path = "/swagger/{documentName}/swagger.json";
});

/// Adds the Swagger UI to the pipeline which can be customized with multiple
/// options. 
/// https://github.com/RicoSuter/NSwag/wiki/AspNetCore-Middleware
app.UseSwaggerUi3(options =>
{
    /// If not explicitly specified, the default route is: /swagger.
    options.Path = "/swagger";
});

#endregion

/// <summary>
/// Configures the application to serve Blazor WebAssembly framework files
/// from the root path "/".
/// https://gist.github.com/ShaunCurtis/0ed8d257dff4d8497b97c88e5b2b30d0
/// </summary>
app.UseBlazorFrameworkFiles();

/// <summary>
/// Enables static files to be served. It marks the files in the web root
/// folder (wwwroot) as servable. Typically, app.UseStaticFiles is called before
/// calling app.UseAuthorization. This means no AuthZ checks are performed on
/// static files and that files in the wwwroot directory are publicly
/// accessible.  
/// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-6.0
/// https://learn.microsoft.com/en-us/aspnet/core/razor-pages/ui-class?view=aspnetcore-5.0&tabs=visual-studio#consume-content-from-a-referenced-rcl-2
/// </summary>
app.UseStaticFiles();

/// Adds route matching by looking at the set of endpoints defined in the app
/// and selecting the best match based on the request.
/// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-3.1#routing-basics-1
app.UseRouting();

/// Adds the RequestLocalizationMiddleware to set culture information for
/// requests based on information provided by the client. Must be called
/// after the UseRouting() extension method. 
app.ConfigureRequestLocalizationPipeline();

/// Adds the middleware to the request processing pipeline to cache Http
/// response content.
/// In apps that use CORS middleware, UseOutputCache must be called after
/// UseCors.
/// In Razor Pages app and apps with controllers, UseOutputCache must be
/// called after UseRouting.
/// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-7.0#add-the-middleware-to-the-app
///
/// It should be placed after the Swagger middleware.
///
/// It is disabled because the application was transformed to adhere to the
/// PWA standard which employs a service worker that also caches data with
/// the added benefit of optionally making it available offline.
//app.UseOutputCache();

/// Exposes the OpenID Connect (OIDC) endpoints.
/// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#startup-class
app.UseIdentityServer();

/// <summary>
/// AuthN will be performed automatically on every call into this host.
///
/// The Authentication middleware is responsible for validating request
/// credentials and setting the user on the request context.
/// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#startup-class
/// https://docs.duendesoftware.com/identityserver/v6/quickstarts/1_client_credentials/#add-jwt-bearer-authentication
/// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#server-app-configuration
/// </summary>
app.UseAuthentication();

/// <summary>
/// Makes sure API endpoints cannot be accessed by anonymous Clients. When
/// authorizing a resource that is routed using endpoint routing, this call
/// must appear between the calls to app.UseRouting() and app.UseEndpoint(...)
/// for the middleware to work correctly.
/// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#startup-class
/// https://docs.duendesoftware.com/identityserver/v6/quickstarts/1_client_credentials/#add-jwt-bearer-authentication 
/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.builder.authorizationappbuilderextensions.useauthorization?view=aspnetcore-6.0
/// </summary>
app.UseAuthorization();

/// <summary>
/// Enables rate limiting for the application. It must be placed after the
/// 'app.UseRouting()' call. 
/// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#limiter-with-onrejected-retryafter-and-globallimiter
/// https://nicolaiarocci.com/on-implementing-the-asp.net-core-7-rate-limiting-middleware/
/// </summary>
app.UseRateLimiter();

/// <summary>
/// Our app uses ASP.Net Core Razor pages for the AuthN and AuthZ UI. This is
/// the configuration middleware for adding endpoints for Razor pages to the
/// IEndpointRouteBuilder.
/// https://docs.microsoft.com/en-us/aspnet/core/razor-pages/?view=aspnetcore-6.0&tabs=visual-studio
/// https://docs.microsoft.com/en-us/aspnet/core/razor-pages/razor-pages-conventions?view=aspnetcore-6.0
/// </summary>
app.MapRazorPages()
    /// Adds the specified rate limiting policy to all Razor pages.
    /// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#enableratelimiting-and-disableratelimiting-attributes
    /// You can rate limit groups of endpoints using specific rate limiting
    /// policies:
    /// https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
    .RequireRateLimiting(RateLimitingServices.RazorPagesRateLimitPolicy);

/// Adds endpoints for controller actions to the IEndpointRouteBuilder without
/// specifying any routes.
app.MapControllers()
    /// Adds the specified rate limiting policy to all MVC controllers.
    /// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#enableratelimiting-and-disableratelimiting-attributes
    /// You can rate limit groups of endpoints using specific rate limiting
    /// policies:
    /// https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
    .RequireRateLimiting(RateLimitingServices.ApiRateLimitPolicy);

app.MapFallbackToFile("index.html");

app.Run();


