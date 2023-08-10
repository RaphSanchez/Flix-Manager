using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;

namespace BlazorMovies.Server.Helpers.ServiceExtensions
{
    /// <summary>
    /// The <see cref="RateLimitingServices"/> class extends the
    /// IServicesCollection of the dependency injection container in the
    /// Application/Server-Api Program class. It contains an extension method
    /// <see cref="ConfigureRateLimitingServices"/> designed to register
    /// custom RateLimiting services.
    /// </summary>
    /// <remarks>
    /// This approach permits a cleaner Application/Server-Api Program class
    /// because most of the code logic is defined within the extension methods.
    /// The <see cref="ConfigureRateLimitingServices"/> extension method
    /// defined here is called from the dependency injection container during
    /// app build up.
    /// <para>
    /// See YouTube video <see href="https://youtu.be/GNJ1EKavzGk">
    /// Add Identity Core To ASP.Net Core API | Ultimate ASP.Net Web API
    /// Tutorial for Beginners
    /// </see> and Implement Rate Limiting Middleware of the
    /// <see href="https://blog.christian-schou.dk/implement-rate-limiting-in-asp-net-core-web-api/">
    /// How to implement Rate Limiting in an ASP.NET Core Web API
    /// </see> blog.
    /// </para>
    /// </remarks>
    public static class RateLimitingServices
    {
        /// <summary>
        /// Class level property that represents a rate limit policy name.
        /// Its value is consumed multiple times including from the
        /// configuration of the Http request pipeline in
        /// Application/Server-Api Program class.
        /// </summary>
        /// <remarks>
        /// Property name and property value <strong>must exactly match</strong>.
        /// Otherwise, the [EnableRateLimiting] and [DisableRateLimiting]
        /// attributes throw an exception.
        /// </remarks>
        internal static string ApiRateLimitPolicy => "ApiRateLimitPolicy";

        /// <summary>
        /// Class level property that represents a rate limit policy name.
        /// Its value is consumed multiple times including from the
        /// configuration of the Http request pipeline in
        /// Application/Server-Api Program class.
        /// </summary>
        /// <remarks>
        /// Property name and property value <strong>must exactly match</strong>.
        /// Otherwise, the [EnableRateLimiting] and [DisableRateLimiting]
        /// attributes throw an exception.
        /// </remarks>
        internal static string RazorPagesRateLimitPolicy =>
            "RazorPagesRateLimitPolicy";

        /// <summary>
        /// Stores the name of the RateLimiter that initiated the rejection
        /// and the partition key if and when a rate limiter operation is
        /// triggered for an Http request.
        ///
        /// It is passed to the body of the Http response and logged into the
        /// debug console by the OnRejected delegate.
        /// </summary>
        private static string? _rateLimiterAndPartitionKey;
        
        /// <summary>
        /// Configures the Rate Limit Services to control the number of Http
        /// requests processed by the Application/Server-Api and the
        /// Application/Server-Api/IdentityServer engine to keep traffic at a
        /// safe rate. 
        /// </summary>
        /// <remarks>
        /// <para>
        /// See <see href="https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#enableratelimiting-and-disableratelimiting-attributes">
        /// Rate limiting middleware in ASP.Net Core
        /// </see>,
        /// <see href="https://blog.elmah.io/built-in-rate-limiting-in-asp-net-core-vs-aspnetcoreratelimit/">
        /// Built-in rate limiting in ASP.Net Core vs AspNetCoreRateLimit
        /// </see>,
        /// <see href="https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html">
        /// ASP.Net Core rate limiting middleware in .Net 7
        /// </see>, YouTube video
        /// <see href="https://youtu.be/vpeddZPkfAw">
        /// Rate Limiting in .Net 7.0: Getting Started
        /// </see> and
        /// <see href="https://learn.microsoft.com/en-us/dotnet/core/extensions/http-ratelimiter">
        /// Rate Limit an HTTP handler in .Net
        /// </see>.
        /// </para>
        /// </remarks>
        /// <param name="services">The IServiceCollection.</param>
        /// <returns>An asynchronous operation.</returns>
        public static void ConfigureRateLimitingServices(
            this IServiceCollection services)
        {
            /// Adds rate limiting services to the
            /// Microsoft.Extensions.DependencyInjection.IServiceCollection
            /// including its configuration options.
            /// https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
            /// https://blog.elmah.io/built-in-rate-limiting-in-asp-net-core-vs-aspnetcoreratelimit/
            services.AddRateLimiter(rateLimiterOptions =>
            {
                /// Customizes how to handle any rate limiting rejected requests.
                rateLimiterOptions.OnRejected =
                    async (context, cancellationToken) =>
                    {
                        /// Sets the response status code.
                        context.HttpContext.Response.StatusCode =
                            StatusCodes.Status429TooManyRequests;

                        /// The OnRejectedContext.Lease property is an
                        /// abstraction that represents the success or failure
                        /// to acquire a resource and contains potential
                        /// metadata that is relevant to the acquisition
                        /// operation.
                        if (context.Lease
                            .TryGetMetadata(
                                MetadataName.RetryAfter,
                                out TimeSpan retryAfter))
                        {
                            /// Adds the RetryAfter Http header name with the
                            /// time period value to wait before attempting to
                            /// generate a new Http request. 
                            /// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#limiter-with-onrejected-retryafter-and-globallimiter
                            context.HttpContext.Response.Headers.Add(
                                HeaderNames.RetryAfter,
                                $"{((int)retryAfter.TotalSeconds)
                                    .ToString(NumberFormatInfo.InvariantInfo)}" +
                                $" seconds");
                        }

                        /// Message passed to the debug console (ILogger) and
                        /// to the Http response body.
                        string requestRejectedMessage =
                            $"Request rejected by " +
                            $"{_rateLimiterAndPartitionKey}. " +
                            $"Please retry after" +
                            $" {(int)retryAfter.TotalSeconds} seconds.";

                        /// Writes a warning log message to the debug console.
                        context.HttpContext.RequestServices
                            .GetService<ILoggerFactory>()?
                            .CreateLogger(
                                "Microsoft.AspNetCore.RateLimitingMiddleware")
                            .LogWarning(requestRejectedMessage);

                        /// Asynchronous method to write the given text to the
                        /// Http response body. It must be placed at the end
                        /// (after synchronous methods) of this "OnRejected"
                        /// section. 
                        await context.HttpContext.Response
                            .WriteAsync(requestRejectedMessage,
                                cancellationToken: cancellationToken);
                    };

                /// Enables rate limiting at a global level; i.e., it is
                /// applied to every HTTP request. You can create "chained"
                /// rate limiters if you need multiple rate limiter options;
                /// e.g., 100 requests per minute and 6000 requests per hour.
                ///
                /// The global limiter will be executed first, followed by
                /// any endpoint-specific limiters if any. 
                /// https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
                /// https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.ratelimiting.ratelimiteroptions.globallimiter?view=aspnetcore-7.0#microsoft-aspnetcore-ratelimiting-ratelimiteroptions-globallimiter
                rateLimiterOptions.GlobalLimiter = PartitionedRateLimiter
                    .Create<HttpContext, string>(
                        partitioner: httpContext =>
                        {
                            /// Uses the "sub" (subject) claim as the
                            /// "partitionKey" for the partitioner or
                            /// "anonymous" is user not authenticated
                            /// or does not contain the claim.
                            string partitionKey =
                                httpContext.User.Claims
                                        /// NameIdentifier =
                                        /// ASP.Net User primary key (GUID).
                                        /// 
                                        /// httpContext.User.Identity?.Name =
                                        /// ASP.Net User email.
                                        .FirstOrDefault(c =>
                                            c.Type == ClaimTypes.NameIdentifier)?
                                        .Value
                                    ?? "Anonymous";

                            /// Captures the value of the RateLimiter and the
                            /// partition key to log in case an Http request is
                            /// rejected. 
                            _rateLimiterAndPartitionKey = string.Empty;
                            _rateLimiterAndPartitionKey =
                                $"'GlobalLimiter' with partition key:" +
                                $" '{partitionKey}'";

                            return RateLimitPartition.GetFixedWindowLimiter(
                                partitionKey: partitionKey,
                                factory: action =>
                                    new FixedWindowRateLimiterOptions
                                    {
                                        PermitLimit = 60,
                                        Window = TimeSpan.FromSeconds(60),

                                        QueueLimit = 0,
                                        QueueProcessingOrder =
                                            QueueProcessingOrder.OldestFirst,
                                    });
                        });

                /// Rate limiting policy applied to MVC Controllers of the
                /// Application/Server-Api. It is explicitly passed as an
                /// argument of the .RequireRateLimiting() extension method
                /// chained to the MapControllers extension method in the
                /// Http request pipeline configuration file:
                /// Application/Server-Api Program class. 
                ///
                /// Rate Limiting Policies allow enabling and disabling rate
                /// limiting at the controller, action method, or Razor Page
                /// level (as opposed to global rate limiters). 
                ///
                /// A rate limiting policy can be defined in line or in a
                /// custom class that derives from IRateLimiterPolicy<T>.
                /// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#enableratelimiting-and-disableratelimiting-attributes
                /// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#limiter-with-onrejected-retryafter-and-globallimiter
                /// https://devblogs.microsoft.com/dotnet/announcing-rate-limiting-for-dotnet/#ratelimiting-middleware
                /// https://blog.maartenballiauw.be/post/2022/09/26/aspnet-core-rate-limiting-middleware.html
                rateLimiterOptions.AddPolicy(
                    policyName: ApiRateLimitPolicy,
                    partitioner: httpContext =>
                    {
                        #region JWT AccessToken partition key
                        /// https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0#limiter-with-authorization

                        /// The Application/Client/ApiServices/ApiManager
                        /// ApiConnector will allocate the HttpClientWithJwt or
                        /// the HttpClientNoJwt to include (or not) a security
                        /// token (JWT) to build the Http request dependent on
                        /// whether the Application/Server-Api resource
                        /// (endpoint) is secured (or not) and on whether the
                        /// current user has been authenticated (or not).
                        string accessToken = httpContext.Features
                                              .Get<IAuthenticateResultFeature>()?
                                              .AuthenticateResult?.Properties?
                                              .GetTokenValue("access_token")?
                                              .ToString()
                                          ?? string.Empty;


                        /// If the requested resource is secured; i.e., if a
                        /// security token is included, it is decoded and used
                        /// to extract the "sub" (subject) claim which
                        /// identifies the ClaimsPrincipal (the identifier for
                        /// the current user).
                        ///
                        /// The appliation uses a GUID as a primary key that
                        /// represents a unique user id for every registered
                        /// user, including users that used an external login
                        /// provider (e.g., Google or Facebook).
                        if (!string.IsNullOrEmpty(accessToken))
                        {
                            /// Converts a string into an instance of type
                            /// JwtSecurityToken.
                            // https://stackoverflow.com/questions/38340078/how-to-decode-jwt-token
                            JwtSecurityTokenHandler handler = new();
                            JwtSecurityToken? jwtSecurityToken = handler
                                    .ReadJwtToken(accessToken);

                            /// Extracts the value for the "sub" (subject)
                            /// claim. The "sub" claim value is the ASP.Net
                            /// User Id of type GUID.
                            string subjectClaimValue = jwtSecurityToken.Claims
                                    .First(claim => claim.Type == "sub")
                                    .Value;

                            /// Captures the value of the RateLimiter and the
                            /// partition key to log in case an Http request is
                            /// rejected. 
                            _rateLimiterAndPartitionKey = string.Empty;
                            _rateLimiterAndPartitionKey =
                                $"'{ApiRateLimitPolicy}' with partition key:" +
                                $" 'SubjectClaim'";

                            /// Uses the "sub" (subject) claim as the
                            /// "partitionKey" for the rate limiting policy. 
                            return RateLimitPartition
                                .GetFixedWindowLimiter(
                                partitionKey: subjectClaimValue,
                                factory: action => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 30,

                                    Window = TimeSpan.FromSeconds(60),

                                    QueueLimit = 1,
                                    QueueProcessingOrder =
                                        QueueProcessingOrder.OldestFirst
                                });
                        }

                        #endregion

                        #region Anonymous user partition key

                        /// Captures the value of the partition key to log
                        /// in case an Http request is rejected. 
                        _rateLimiterAndPartitionKey = string.Empty;
                        _rateLimiterAndPartitionKey =
                            $"'{ApiRateLimitPolicy}' with partition key:" +
                            $" 'Anonymous'";

                        /// If the requested resource is NOT secured; i.e.,
                        /// if a security token is not included in the Http
                        /// request, all users receive the rate limit options
                        /// defined by this RateLimitPartition. 
                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: "Anonymous",
                            factory: action => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 60,
                                Window = TimeSpan.FromSeconds(60),

                                QueueLimit = 1,
                                QueueProcessingOrder =
                                    QueueProcessingOrder.OldestFirst,
                            });

                        #endregion
                    });

                /// The app uses ASP.Net Core Razor pages for the AuthN and
                /// AuthZ UI.
                ///
                /// Rate limiting policy applied to Razor Pages of the
                /// Application/Server-Api/Areas/Identity Pages. It is
                /// explicitly passed as an argument of the
                /// .RequireRateLimiting() extension method chained to the
                /// MapRazorPages extension method in the Http request
                /// pipeline configuration file:
                /// Application/Server-Api Program class. 
                rateLimiterOptions.AddPolicy(
                    policyName: RazorPagesRateLimitPolicy,
                    partitioner: httpContext =>
                    {
                        /// Uses the "sub" (subject) claim as the
                        /// "partitionKey" for the partitioner or
                        /// "anonymous" is user not authenticated
                        /// or does not contain the claim.
                        string partitionKey =
                            httpContext.User.Claims
                                /// NameIdentifier =
                                /// ASP.Net User primary key (GUID).
                                /// 
                                /// httpContext.User.Identity?.Name =
                                /// ASP.Net User email.
                                .FirstOrDefault(c =>
                                    c.Type == ClaimTypes.NameIdentifier)?
                                .Value
                            ?? "anonymous";

                        /// Captures the value of the partition key to log
                        /// in case an Http request is rejected. 
                        _rateLimiterAndPartitionKey = string.Empty;
                        _rateLimiterAndPartitionKey =
                            $"'{RazorPagesRateLimitPolicy}' with partition key:" +
                            $" '{partitionKey}'";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey: partitionKey,
                            factory: action =>
                                new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 60,
                                    Window = TimeSpan.FromSeconds(60),

                                    QueueLimit = 1,
                                    QueueProcessingOrder =
                                        QueueProcessingOrder.OldestFirst,
                                });
                    });
            });
        }
    }
}



