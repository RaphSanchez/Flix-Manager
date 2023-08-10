using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorMovies.Shared.EDM
{
    /// <summary>
    /// A wrapper for the message delivery context established between the user
    /// agent (computer program representing a person; e.g., the browser) and
    /// the push service on behalf of a web application. In other words, it is
    /// the subscription object returned by the push service of the user agent
    /// when an application user is subscribed.
    /// </summary>
    /// <remarks>
    /// It represents a record in the PushSubscriptionsDetails database table
    /// with the required data to target a specific end user to send a push
    /// notification. The PushSubscriptionsDetails is a store for the end users
    /// that have subscribed to our push notification system; i.e., users that
    /// have granted permission to receive push notifications from our app.
    /// <para>
    /// Ensure that the property names exactly match the property names of the
    /// "extractSubscriptionDetails" function in the
    /// Application/Client/wwwroot/js push-notifications.js file. 
    /// </para>
    /// <para>
    /// Episode "154. Push API - Backend" of Udemy course <see href="https://www.udemy.com/share/101ZK23@6p5fxNEkabk-TEVvkFtbFg5VoKT-KyAn5kT6flONTaBmMTVtpPrI4sOl_LvxAvSo/">
    ///  Programando en Blazor - ASP.Net Core 7</see>,
    /// <see href="https://www.w3.org/TR/push-api/#push-subscription">
    /// Push subscription</see>.
    /// </para>
    /// </remarks>
    public class PushSubscriptionDetails
    {
        /// <summary>
        /// Primary key or identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Provided by the push service (web browser). This is where the app
        /// server can send push messages to the push service which in turn
        /// delivers the push message to the end user.
        /// </summary>
        /// <remarks>
        /// Property name must exactly match the property name of the
        /// "extractSubscriptionDetails" function in the
        /// Application/Client/wwwroot/js push-notifications.js file.
        /// </remarks>
        public string PushEndpointUrl { get; set; } = null!;

        /// <summary>
        /// Provided by the push service (web browser). Used to encrypt and
        /// authenticate push messages. Each elliptic curve Diffie-Hellman
        /// (ECDH) public key is associated with a
        /// <see cref="PushSubscriptionDetails"/> record for a specific
        /// application user.
        /// </summary>
        /// <remarks>
        /// Property name must exactly match the property name of the
        /// "extractSubscriptionDetails" function in the
        /// Application/Client/wwwroot/js push-notifications.js file.
        /// </remarks>
        public string P256dh { get; set; } = null!;

        /// <summary>
        /// Provided by the push service (web browser). The authentication
        /// secret is used by the Application/Server-Api to encrypt and
        /// authenticate the push messages.
        /// </summary>
        /// <remarks>
        /// Property name must exactly match the property name of the
        /// "extractSubscriptionDetails" function in the
        /// Application/Client/wwwroot/js push-notifications.js file.
        /// </remarks>
        public string Auth { get; set; } = null!;
    }
}

