using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;

namespace BlazorMovies.Client.ApiServices.ApiManager
{
    /// <summary>
    /// One application specific ApiEntityName class for each
    /// IEntityName interface exposed in the <see cref="IApiService"/>
    /// interface.
    /// </summary>
    /// <remarks>
    /// It is a subclass of the <see cref="ApiRepository{TEntity}"/>
    /// class which means it inherits its general functionality
    /// applicable to all data entities. 
    /// <para>
    /// This class is application specific and extends its base class
    /// with specific functionality for the type passed as type parameter.
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in
    /// the result of a query with EF's "Include" extension method.
    /// </para>
    /// <para>
    /// Its methods have an "explicit interface implementation" to hide
    /// them from unwanted consumers. 
    /// </para>
    /// </remarks>
    internal class ApiPushSubscriptions
        : ApiRepository<PushSubscriptionDetails>, IPushSubscriptions
    {
        /// <summary>
        /// The name of the Application/Server-Api/Controller of the resource
        /// (data entity).
        /// </summary>
        private const string ControllerName = "pushsubscriptions";

        /// <summary>
        /// Its formal input parameter <paramref name="apiConnector"/> is not
        /// stored in a local variable because it is not consumed like that.
        /// Instead, it is passed to satisfy its base class's constructor and
        /// it is that parent class which consumes it and also makes it
        /// available to any child class through a field named
        /// <see cref="ApiConnector"/> with a "<c>protected read-only</c>"
        /// access modifier. 
        /// </summary>
        /// <remarks>
        /// This structure ensures that a complete business transaction
        /// can have multiple operations with different entity types 
        /// using a single instance of a class that implements the
        /// <see cref="IApiConnector"/> interface which in turn employs a
        /// single instance of the <see cref="HttpClient"/> class to avoid
        /// exhausting the web sockets under heavy loads.
        /// </remarks>
        /// <param name="apiConnector">Instance responsible for building
        /// the URI to map to the Application/Server-Api controller and
        /// for sending/receiving Http requests/responses.</param>
        public ApiPushSubscriptions(IApiConnector apiConnector)
        : base(ControllerName, apiConnector)
        { }

        #region Post-Create actions

        /// <summary>
        /// Sends an Http request to persist into the PushSubscriptionsDetails
        /// database table a record with the required data to target a specific
        /// end user to send a push notification. 
        /// </summary>
        /// <remarks>
        /// Note that it is passed a <strong>JwtOptions.OmitJWTs</strong>
        /// argument to build the Http request without a security JWT. This
        /// allows any user to subscribe to the web push notifications service.
        /// <para>
        /// Subscribing an application user to the push service of the web
        /// browser is a two step process:
        /// </para>
        /// <para>
        /// 1. <see cref="PushNotification"/> component uses JSInterop to
        /// subscribe the user.
        /// </para>
        /// <para>
        /// 2. <see cref="PushNotification"/> component consumes this
        /// <see cref="IPushSubscriptions.AddPushSubscriptionAsync"/> to
        /// persist the related data into the PushSubscriptionsMetadata database
        /// table.
        /// </para>
        /// </remarks>
        /// <param name="pushSubscriptionDetails">The object that wraps the
        /// data required to target a specific end user to send a push
        /// notification.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the PushSubscriptionDetails object successfully inserted.
        /// </returns>
        async Task<PushSubscriptionDetails>
            IPushSubscriptions.AddPushSubscriptionAsync(
                PushSubscriptionDetails? pushSubscriptionDetails)
        {
            try
            {
                if (pushSubscriptionDetails is null)
                    throw new ArgumentNullException(
                        nameof(pushSubscriptionDetails));

                /// The Application/Server-Api/Controllers/PushSubscriptions
                /// controller decorates its AddPushSubscriptionTask action
                /// (method) with an [HttpPost] route template that includes a
                /// "push-notifications-subscribe" route segment.
                ///
                /// The ApiConnector class responsible for building the URL for
                /// the HTTP request includes the route segment to indicate .Net
                /// Core routing middleware to dispatch the HTTP request to the
                /// action in the PushSubscriptions controller that matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = "/push-notifications-subscribe";

                /// Consumes an ApiConnector resource method with the details
                /// of building an Http POST request for the appropriate
                /// endpoint (controller action's route template).
                ///
                /// Note that it is passed a JwtOptions.OmitJWTs argument to
                /// build the Http request without a security JWT. This allows
                /// any user to subscribe to the web push notifications service.
                PushSubscriptionDetails insertedPushSubscriptionDetails =
                    await ApiConnector
                        .InvokePostAsync<PushSubscriptionDetails>(
                            pushSubscriptionDetails,
                            ControllerName,
                            routeTemplateComplement,
                            jwtOptions: JwtOptions.OmitJWTs);

                return insertedPushSubscriptionDetails;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers PushSubscriptions
                /// controller action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        #endregion

        #region Get-Read actions

        /// <summary>
        /// Sends an Http request to retrieve the VAPID Public Key. 
        /// </summary>
        /// <remarks>
        /// Voluntary Application Server Identity (VAPID) is a way to send and
        /// receive website push notifications. The Application/Server-Api
        /// voluntarily identifies itself to the push service using VAPID.
        /// <para>
        /// An alternative to this approach is to create a minimal API. Refer
        /// to "Episode 154. Push API - Backend" of Udemy course
        /// <see href="https://www.udemy.com/share/101ZK23@FHXR8HROjz7zUmlmb0LdPBR7qBb3ffpRwyKadwoOp833xq3Gp8pRfnTVDJ6xaO9s/">
        /// Programando en Blazor - ASP.Net Core 7
        /// </see> by Felipe Gavilán and
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0">
        /// Tutorial: Create a minimal API with ASP.Net Core</see>.
        /// </para>
        /// </remarks>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the public key successfully retrieved from the application
        /// secrets. 
        /// </returns>
        public async Task<string> GetVapidPublicKeyAsync()
        {
            try
            {
                string routeTemplateComplement = "/get-public-key";

                string vapidPublicKey = await ApiConnector.InvokeGetAsync<string>(
                        ControllerName,
                        routeTemplateComplement,
                        jwtOptions: JwtOptions.OmitJWTs);

                return vapidPublicKey;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers MoviesController action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating it
                /// up in the stack. 
                throw;
            }
        }

        #endregion

        #region Put-Update actions

        #endregion

        #region Delete actions

        /// <summary>
        /// Sends an Http request to remove from the PushSubscriptionsDetails
        /// database table a record with the required data to target a specific
        /// end user to send a push notification.
        /// </summary>
        /// <remarks>
        /// Note that it is passed a <strong>JwtOptions.OmitJWTs</strong>
        /// argument to build the Http request without a security JWT. This
        /// allows any user to unsubscribe from the web push notifications
        /// service.
        /// <para>
        /// Unsubscribing an application user from the push service
        /// of the web browser is a two step process:
        /// </para>
        /// <para>
        /// 1. <see cref="PushNotification"/> component uses JSInterop to unsubscribe
        /// the user.
        /// </para>
        /// <para>
        /// 2. <see cref="PushNotification"/> component consumes this
        /// <see cref="IPushSubscriptions.DeletePushSubscriptionAsync"/> to
        /// remove the related data from the PushSubscriptionsMetadata database
        /// table.
        /// </para>
        /// </remarks>
        /// <param name="pushSubscriptionDetails">The object that wraps the
        /// data required to target a specific end user to send a push
        /// notification.</param>
        /// <returns>The deserialized JSON content from the response message;
        /// i.e., the PushSubscriptionDetails object successfully deleted.
        /// </returns>
        async Task<PushSubscriptionDetails>
            IPushSubscriptions.DeletePushSubscriptionAsync(
                PushSubscriptionDetails? pushSubscriptionDetails)
        {
            try
            {
                if (pushSubscriptionDetails is null)
                    throw new ArgumentNullException(
                        nameof(pushSubscriptionDetails));

                /// The Application/Server-Api/Controllers/PushSubscriptions
                /// controller decorates its DeletePushSubscriptionTask action
                /// (method) with an [HttpPost] route template that includes a
                /// "push-notifications-unsubscribe" route segment.
                ///
                /// The ApiConnector class responsible for building the URL for
                /// the HTTP request includes the route segment to indicate .Net
                /// Core routing middleware to dispatch the HTTP request to the
                /// action in the PushSubscriptions controller that matches.
                /// https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-6.0#attribute-routing-with-http-verb-attributes
                /// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-6.0
                /// https://chrissainty.com/working-with-query-strings-in-blazor/
                /// https://web.archive.org/web/20151229061347/http://blog.lunatech.com/2009/02/03/what-every-web-developer-must-know-about-url-encoding
                /// https://stackoverflow.com/questions/2322764/what-characters-must-be-escaped-in-an-http-query-string
                string routeTemplateComplement = "/push-notifications-unsubscribe";

                /// Consumes an ApiConnector resource method with the details
                /// of building an Http POST request for the appropriate
                /// endpoint (controller action's route template).
                ///
                /// Note that it is passed a JwtOptions.OmitJWTs argument to
                /// build the Http request wihtout a security JWT. This allows
                /// any user to subscribe to the web push notifications service.
                PushSubscriptionDetails deletedPushSubscriptionDetails =
                    await ApiConnector.InvokePostAsync<PushSubscriptionDetails>(
                        pushSubscriptionDetails,
                        ControllerName,
                        routeTemplateComplement,
                        JwtOptions.OmitJWTs);

                return deletedPushSubscriptionDetails;
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and sent it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// The ApiConnector class employed to deserialize the Http
                /// response evaluates if the response was successful. If not,
                /// it produces an HttpRequestException and includes the
                /// deserialized message sent from the
                /// Application/Server-Api/Controllers PushSubscriptions
                /// controller action.
                /// 
                /// The message can ultimately be consumed to inform the
                /// application user of the error. For this reason, the
                /// HttpRequestException is thrown back to continue propagating
                /// it up in the stack. 
                throw;
            }
        }

        #endregion
    }
}


