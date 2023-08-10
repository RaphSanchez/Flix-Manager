
using BlazorMovies.Shared;

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Custom class enables managing the VAPID details (sensitive information)
    /// to configure the <see cref="PushNotificationsService"/>.
    /// </summary>
    /// <remarks>
    /// Voluntary Application Server Identity (VAPID) is a way to send and
    /// receive website push notifications. The Application/Server-Api
    /// voluntarily identifies itself to the push service using VAPID.
    /// <para>
    /// VAPID uses JWT to contain a set of information or claims that describe
    /// the sender of the data. 
    /// </para>
    /// <para>
    /// See <see href="https://blog.mozilla.org/services/2016/08/23/sending-vapid-identified-webpush-notifications-via-mozillas-push-service/">
    /// Creating your VAPID claim</see> and video 
    /// <see href="https://youtu.be/c25PDH7ZJfk">
    /// 31. Use VAPID to secure push messages - Progressive Web App Training
    /// </see> of YouTube course "Progressive Web App Training" by Google Chrome
    /// Developers.
    /// </para>
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-6.0">
    /// Azure Key Vault</see>,
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0">
    /// Safe storage of app secrets in development in ASP.Net Core</see>, and
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0">
    /// Options Pattern</see>.
    /// </para>
    /// </remarks>
    public class VapidOptions
    {
        /// <summary>
        /// Enables the push service operator to know who is sending the
        /// notifications and how to contact the sender in case something goes
        /// wrong.
        /// </summary>
        /// <remarks>
        /// See <see href="https://stackoverflow.com/questions/40392257/what-is-vapid-and-why-is-it-useful">
        /// What is VAPID and why is it useful?
        /// </see>
        /// </remarks>
        public string VapidEmail { get; set; } = string.Empty;

        /// <summary>
        /// Required by the push service to validate the sender of the push
        /// notification.
        /// </summary>
        public string VapidPublicKey { get; set; } = string.Empty;

        /// <summary>
        /// Required by the Application/Server-Api
        /// <see cref="PushNotificationsService"/> to sign the push messages.
        /// </summary>
        public string VapidPrivateKey { get; set; } = string.Empty;
    }
}

