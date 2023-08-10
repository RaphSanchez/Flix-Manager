using System.Text.Json;
using BlazorMovies.Server.DataStore;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using WebPush;

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Abstract layer that establishes a contract to allow the
    /// Application/Server-Api to send push messages to the servers of the web
    /// browser employed by the end user. The browser's push service in turn
    /// transmits the message to the end user. It provides the push endpoint(s)
    /// (specific URL for a given end user) for the push subscription(s) it
    /// serves.
    /// </summary>
    /// <remarks>
    /// See episode "154. Push API - Backend" of Udemy course <see href="https://www.udemy.com/share/101ZK23@6p5fxNEkabk-TEVvkFtbFg5VoKT-KyAn5kT6flONTaBmMTVtpPrI4sOl_LvxAvSo/">
    ///  Programando en Blazor - ASP.Net Core 7</see>,
    /// <see href="https://www.w3.org/TR/push-api/#push-subscription">
    /// Push subscription</see>, and
    /// <see href="https://github.com/dotnet-presentations/blazor-workshop/blob/main/docs/09-progressive-web-app.md#sending-push-notifications">
    /// Sending push notifications</see>.
    /// </remarks>
    public interface IPushNotificationsService
    {
        /// <summary>
        /// Sends a push message to inform the application users that a new
        /// movie is in theaters. 
        /// </summary>
        /// <remarks>
        /// Only users that have previously 'granted' permission to receive
        /// push notifications will be notified.
        /// </remarks>
        /// <param name="movie">The <see cref="Movie"/> object that has been
        /// recently added and flagged as "In Theaters".</param>
        /// <returns>An asynchronous operation.</returns>
        Task SendPushMessageMovieOnTheatersAsync(Movie movie);
    }

    /// <summary>
    /// Allows the Application/Server-Api to send push messages to the servers
    /// of the web browser employed by the end user. The browser's push service
    /// in turn transmits the message to the end user. It provides the push
    /// endpoint(s) (specific URL for a given end user) for the push
    /// subscription(s) it serves. 
    /// </summary>
    /// <remarks>
    /// Episode "154. Push API - Backend" of Udemy course <see href="https://www.udemy.com/share/101ZK23@6p5fxNEkabk-TEVvkFtbFg5VoKT-KyAn5kT6flONTaBmMTVtpPrI4sOl_LvxAvSo/">
    ///  Programando en Blazor - ASP.Net Core 7</see>,
    /// <see href="https://www.w3.org/TR/push-api/#push-subscription">
    /// Push subscription</see> and
    /// <see href="https://github.com/dotnet-presentations/blazor-workshop/blob/main/docs/09-progressive-web-app.md#sending-push-notifications">
    /// Sending push notifications</see>.
    /// </remarks>
    public class PushNotificationsService : IPushNotificationsService
    {
        /// <summary>
        /// VAPID details required to configure the
        /// <see cref="PushNotificationsService"/>. They have been safely
        /// stored in App Secrets using the Secret Manager.
        /// </summary>
        private readonly VapidOptions _options;

        /// <summary>
        /// The database context.
        /// </summary>
        private readonly AppDbContext _context;

        /// <summary>
        /// Injects required instances for the PushNotificationsService.
        /// </summary>
        /// <param name="optionsAccessor">Custom class to manage the VAPID details
        /// (sensitive information) to configure the
        /// <see cref="PushNotificationsService"/>.</param>
        /// <param name="context">The database context.</param>
        public PushNotificationsService(IOptions<VapidOptions> optionsAccessor,
            AppDbContext context)
        {
            _options = optionsAccessor.Value;
            _context = context;
        }

        /// <summary>
        /// Sends a push message to inform the application users that a new
        /// movie is in theaters. 
        /// </summary>
        /// <remarks>
        /// Only users that have previously 'granted' permission to receive
        /// push notifications will be notified.
        /// </remarks>
        /// <param name="movie">The <see cref="Movie"/> object that has been
        /// recently added and flagged as "In Theaters".</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task SendPushMessageMovieOnTheatersAsync(Movie movie)
        {
            /// Collection of push subscriptions details; i.e., end users that
            /// granted permission to receive push messages from our app. 
            List<PushSubscriptionDetails> pushSubscriptionsDetails =
                await _context.PushSubscriptionsDetails!.ToListAsync();

            /// Enables the push service operator to know who is sending the
            /// push messages and how to contact the sender in case something
            /// goes wrong.
            ///
            /// Ensure the Email string starts with 'mailto:' or the system
            /// will throw an exception.
            /// https://webpushdemo.azurewebsites.net/
            /// https://github.com/web-push-libs/web-push-csharp/
            string email = _options.VapidEmail;

            /// Required by the push service to validate the JWT. VAPID uses
            /// JWT to contain a set of information or claims that describe the
            /// sender of the data. 
            string publicKey = _options.VapidPublicKey;

            /// Required by the <see cref="PushNotificationsService"/> to sign
            /// the push message.
            string privateKey = _options.VapidPrivateKey;

            /// Wraps the elements used by the Application/Server-Api to
            /// voluntarily identify itself to the push service.
            VapidDetails vapidDetails = new(email, publicKey, privateKey);

            /// WebPush NuGet package used to instantiate a
            /// WebPushClient object that allows sending the push messages.
            ///
            /// WebPushClient implements IDisposable.
            /// https://github.com/web-push-libs/web-push-csharp/
            using WebPushClient webPushClient = new();

            foreach (PushSubscriptionDetails subscription in
                     pushSubscriptionsDetails)
            {
                /// WebPush NuGet package used to construct a push subscription
                /// object that contains the details for a subscription to send
                /// push messages to.
                /// https://github.com/web-push-libs/web-push-csharp/
                PushSubscription pushSubscription = new(
                    subscription.PushEndpointUrl,
                    subscription.P256dh,
                    subscription.Auth);
                try
                {
                    /// Converts the value of a type specified by a generic
                    /// type parameter into a JSON string. Returns a JSON
                    /// string representation of the value.
                    ///
                    /// This example uses an inline anonymous type to
                    /// encapsulate a set of properties into a single object
                    /// without having to explicitly define a type (or class)
                    /// first. 
                    /// 
                    /// Serializes the Movie properties that constitute the
                    /// payload or data of the push message into a JSON string
                    /// representation of the value (or anonymour type). 
                    ///
                    /// Anonymous type's property names must identical to the
                    /// property names in the deserialized payload of the 'push'
                    /// event handler (or listener) in the
                    /// Application/Client/wwwroot service-worker.js and
                    /// service-worker.published.js files.
                    string payload = JsonSerializer.Serialize(new
                    {
                        title = movie.TitleSummary,
                        image = movie.PosterPath,
                        /// Url obtained from RenderMovie component. The end
                        /// user will be redirected here when the push
                        /// notification's click event is raised.
                        url =
                            $"movies/bulletin/{movie.Id}/" +
                            $"{movie.Title?.ToLower().Replace(' ', '-')}"
                    });

                    /// Formulates an Http POST delivery request to the push
                    /// service adhering to the Web Push Protocol and encrypting
                    /// the dat sent according to the Message Encryption for
                    /// Web Push specifications.
                    /// https://github.com/web-push-libs/web-push-csharp/
                    await webPushClient.SendNotificationAsync(
                        pushSubscription, payload, vapidDetails);
                }
                catch (Exception ex)
                {
                    /// Extracts the complete information of the exception and
                    /// logs it into the console.
                    ExceptionLoggers.ExtractAndDisplayException(ex);

                    /// Throws back the exception up the stack.
                    throw;
                }
            }
        }
    }
}


