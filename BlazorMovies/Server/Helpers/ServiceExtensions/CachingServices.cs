
namespace BlazorMovies.Server.Helpers.ServiceExtensions
{
    /// <summary>
    /// The <see cref="CachingServices"/> class extends the IServiceCollection
    /// of the dependency injection container in the Application/Server-Api
    /// Program class. It contains an extension method
    /// <see cref="ConfigureOutputCachingServices"/> designed to register
    /// custom output caching services.
    /// </summary>
    /// <remarks>
    /// This approach permits a cleaner Application/Server-Api Program class
    /// because most of the code logic is defined withing the extension
    /// methods. The <see cref="ConfigureOutputCachingServices"/> extension
    /// method defined here is called from the dependency injection container
    /// during app build up.
    /// </remarks>
    public static class CachingServices
    {
        /// <summary>
        /// Class level property that represents a cache policy name. Its
        /// value can be consumed to pass as an argument to an [OutputCache]
        /// attribute at the controller action, controller, and or Razor page
        /// level.
        /// </summary>
        /// <remarks>
        /// It can also be passed as an argument to the .CacheOutput extension
        /// method applied to the app.MapRazorPages and/or app.MapControllers
        /// in the configuration of the Http request pipeline in
        /// Application/Server-Api program class to enforce the named policy
        /// to a group of Razor pages and/or MVC controllers.
        /// </remarks>
        internal static string NoCachePolicy => "NoCachePolicy";

        /// <summary>
        /// Class level property that represents a cache policy name. Its
        /// value can be consumed to pass as an argument to an [OutputCache]
        /// attribute at the controller action, controller, and or Razor page
        /// level.
        /// </summary>
        /// <remarks>
        /// It can also be passed as an argument to the .CacheOutput extension
        /// method applied to the app.MapRazorPages and/or app.MapControllers
        /// in the configuration of the Http request pipeline in
        /// Application/Server-Api program class to enforce the named policy
        /// to a group of Razor pages and/or MVC controllers.
        /// </remarks>
        internal static string OneDayCachePolicy => "OneDayCachePolicy";

        /// <summary>
        /// Class level property that represents a tag used to identify a
        /// group of Application/Server-Api endpoints. Its value can be
        /// passed as an argument to the IOutputCacheStore.EvictByTagAsync
        /// method to evict (or purge) the cached entries related to the
        /// endpoints in the group.
        /// </summary>
        /// <remarks>
        /// The endpoints that conform the group are defined within a
        /// BasePolicy.
        /// </remarks>
        internal static string MoviesEndpointsTag => "MoviesEndpointsTag";

        /// <summary>
        /// Class level property that represents a tag used to identify a
        /// group of Application/Server-Api endpoints. Its value can be
        /// passed as an argument to the IOutputCacheStore.EvictByTagAsync
        /// method to evict (or purge) the cached entries related to the
        /// endpoints in the group.
        /// </summary>
        /// <remarks>
        /// The endpoints that conform the group are defined within a
        /// BasePolicy.
        /// </remarks>
        internal static string GenresEndpointsTag => "GenresEndpointsTag";

        /// <summary>
        /// Class level property that represents a tag used to identify a
        /// group of Application/Server-Api endpoints. Its value can be
        /// passed as an argument to the IOutputCacheStore.EvictByTagAsync
        /// method to evict (or purge) the cached entries related to the
        /// endpoints in the group.
        /// </summary>
        /// <remarks>
        /// The endpoints that conform the group are defined within a
        /// BasePolicy.
        /// </remarks>
        internal static string PeopleEndpointsTag => "PeopleEndpointsTag";

        /// <summary>
        /// Adds the output caching to the IServiceCollection and configures
        /// how to cache response data. 
        /// </summary>
        /// <param name="services">The IServiceCollection.</param>
        public static void ConfigureOutputCachingServices(
            this IServiceCollection services)
        {
            /// Add the Output Cache service
            /// https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-7.0#add-the-middleware-to-the-app
            services.AddOutputCache(outputCacheOptions =>
            {
                #region Default OutputCacheOptions values

                /// The following OutputCacheOptions property values are the
                /// default output cache values. They are explicitly defined for
                /// demonstration purposes.

                /// Size limit for the output cache in bytes. When exceeded, no
                /// new responses will be cached until older entries are
                /// evicted. Default value is set to 100MB.
                outputCacheOptions.SizeLimit = 100 * 1024 * 1024;

                /// The largest cacheable size for the response in bytes. If
                /// the response exceeds this limit, it will not be cached. The
                /// default is set to 64 MB.
                outputCacheOptions.MaximumBodySize = 64 * 1024 * 1024;

                /// The duration a response is cached when no specific value is
                /// defined by a policy.
                outputCacheOptions.DefaultExpirationTimeSpan =
                    TimeSpan.FromSeconds(60);

                /// The default is to treat paths as case insensitive.
                outputCacheOptions.UseCaseSensitivePaths = false;

                #endregion

                /// Builds and adds an IOutputCachePolicy instance to
                /// base policies. It is applied to all Http responses
                /// that adhere to the following default output caching
                /// policy rules:
                /// 1. Only HTTP 200 responses are cached.
                /// 2. Only HTTP GET or HEAD requests are cached (accessor
                ///     endpoints).
                /// 3. Responses that set cookies are not cached.
                /// 4. Responses to authenticated requests are not cached.
                outputCacheOptions.AddBasePolicy(policyBuilderOptions =>
                {
                    /// OutputDefaultExpirationTimeSpan declared above is 60
                    /// seconds unless otherwise explicitly specified with
                    /// the Expire method. 
                    policyBuilderOptions.Expire(TimeSpan.FromSeconds(60));

                });

                /// Builds and adds an IOutputCachePolicy instance to
                /// base policies. It is applied to all Http responses
                /// that adhere to the default output caching policy rules
                /// and to the requirements established by the .With method.
                outputCacheOptions.AddBasePolicy(policyBuilderOptions =>
                {
                    /// The ".With" method accepts a predicate delegate that
                    /// adds a requirement to the current policy.
                    ///
                    /// The ".Tag" method accepts a string[] of tag(s) to add
                    /// to the cached response.
                    ///
                    /// The tag(s) can be passed as an argument to the
                    /// IOutputCacheStore.EvictByTagAsync method to evict cache
                    /// responses that adhere to the requirements established
                    /// by the predicate delegate. 
                    policyBuilderOptions
                        .With(outputCacheContext =>
                            outputCacheContext.HttpContext.Request.Path
                                .StartsWithSegments("/api/movies"))
                        .Tag(MoviesEndpointsTag);

                    policyBuilderOptions
                        .With(outputCacheContext =>
                            outputCacheContext.HttpContext.Request.Path
                                .StartsWithSegments("/api/genres"))
                        .Tag(MoviesEndpointsTag);

                    policyBuilderOptions
                        .With(outputCacheContext =>
                            outputCacheContext.HttpContext.Request.Path
                                .StartsWithSegments("/api/people"))
                        .Tag(MoviesEndpointsTag);
                });
                
                /// Defines an IOutputCachePolicy which can be referenced by
                /// name to disable caching at the controller action, controller,
                /// and/or Razor page level. 
                outputCacheOptions.AddPolicy(NoCachePolicy,
                    policyBuilderOptions =>
                    {
                        /// Clears the policies and adds one preventing any
                        /// caching logic to happen.
                        ///
                        /// The cache key will never be computed.
                        policyBuilderOptions.NoCache();
                    });

                /// Defines an IOutputCachePolicy which can be referenced by
                /// name to override caching expiration time at the controller
                /// action, controller, and/or Razor page level. 
                outputCacheOptions.AddPolicy(OneDayCachePolicy,
                    policyBuilderOptions =>
                    {
                        /// OutputDefaultExpirationTimeSpan declared above is 60
                        /// seconds unless otherwise explicitly specified with
                        /// the Expire method. 
                        policyBuilderOptions.Expire(TimeSpan.FromDays(1));
                    });
            });
        }
    }
}