using BlazorMovies.Client.ApiServices.ApiManager;
using BlazorMovies.Client.Helpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.Helpers;

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Subscribes/unsubscribes application users to the web push service of
    /// the user agent (a computer program representing a person; e.g., a
    /// browser).
    /// </summary>
    public partial class PushNotifications
    {
        /// <summary>
        /// Captures the status of the permission provided by the current user
        /// to receive web push notifications.
        /// </summary>
        private string _pushNotificationPermission = string.Empty;

        /// <summary>
        /// Represents an instance of a JavaScript runtime to which calls may
        /// be dispatched. 
        /// </summary>
        [Inject]
        private IJSRuntime JsRuntime { get; set; } = null!;

        /// <summary>
        /// Exposes one IEntityName interface for each data entity mapped to
        /// the database. 
        /// </summary>
        [Inject]
        private IApiService ApiService { get; set; } = null!;

        protected override async Task OnInitializedAsync()
        {
            try
            {
                /// Uses JSInterop to retrieve the status of the permission
                /// provided by the current user to receive web push
                /// notifications. The value can be 'granted', 'denied' or
                /// 'default' which stands for neither granted nor denied.
                ///
                /// Refer to Application/Client/wwwroot/js push-notifications.js
                /// file and "Episode 155. Push API - Frontend" of Udemy course
                /// Programando en Blazor - ASP.Net Core 7</see> by Felipe
                /// Gavilán.
                /// https://www.udemy.com/share/101ZK23@YrCDF1LzB9xpEPReoWfAeEfW5Dgcw24qgKVUp5CCbuoWSebyL3OD9dz4D8gKjFCp/
                _pushNotificationPermission =
                    await JsRuntime.GetStatusNotificationPermissionAsync();
            }
            catch (Exception ex)
            {
                /// Extracts the complete information of the exception passed
                /// as an argument including any inner exceptions. It employs
                /// a <see cref="StringBuilder"/> to construct the information
                /// and send it to the web browser's console for display.
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// Apple supports web push as of Safari 16 on macOS Ventura by
                /// default. iOS supports web push as of update 16.4 if, and
                /// only if, the web app is added to the hombe screen.
                ///
                /// In other words, if the platform does not support web push
                /// notifications, this alert will be edisplayed whenever the
                /// PushNotifications component is loaded. This is obviously
                /// not a good idea.
                //await JsRuntime.SwAlConfirmDialogAsync(
                //    "Info",
                //    "Permission status for web push notifications is " +
                //    "unavailable.",
                //    SwAlIconType.info);
            }
        }

        /// <summary>
        /// Event handler for the onclick event of bell icon button element
        /// of the 'default' push notification permission status block.
        /// </summary>
        /// <remarks>
        /// A 'default' status means that the application user has neither
        /// 'granted' nor 'denied' permission to receive push notifications.
        /// Therefore, if the 'onclick' event of the bell icon is raised, it
        /// means that the application user requested to be subscribed to the
        /// push service of the user agent.
        /// </remarks>
        /// <returns>An asynchronous operation. If successful, it includes
        /// notifying the component that its state has changed to trigger a
        /// re-render.</returns>
        private async Task SubscribeToPushNotificationsAsync()
        {
            try
            {
                /// Subscribes the current user to the push service of the user
                /// agent (a computer program representing a person; e.g., a
                /// web browser). Returns the subscription details as a .Net
                /// PushSubscriptionDetails instance with the PushEndpointUrl
                /// as a string and the P256dh and Auth keys as strings encoded
                /// in Base64 format.
                ///
                /// If application is offline, it throws a JSException from the
                /// Application/Client/wwwroot/js push-notification.js file.
                PushSubscriptionDetails? subscriptionDetails =
                    await JsRuntime.SubscribeUserToPushNotificationsAsync();

                if (subscriptionDetails != null)
                {
                    /// Inserts the push subscription details into the database.
                    /// 
                    /// The AddPushSubscriptionAsync method of the
                    /// <see cref="ApiPushSubscriptions"/> class encapsulates
                    /// an <see cref="ApiConnector">.InvokePostAsync resource
                    /// method that does not include a security JWT eventhough
                    /// it is a POST action.
                    await ApiService.PushSubscriptions
                        .AddPushSubscriptionAsync(subscriptionDetails);

                    /// Updates the flag responsible for capturing the current
                    /// status for the notifications permission.
                    _pushNotificationPermission =
                        await JsRuntime.GetStatusNotificationPermissionAsync();

                    /// Informs the user that the subscription was executed
                    /// successfully.
                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Info",
                        "Successfully subscribed to web push notifications.",
                        SwAlIconType.success);

                    /// Re-renders the component to update any recently assigned
                    /// parameter values.
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// subscribeUserToPushNotifications() function in
                /// Application/Client/wwwroot/js push-notification.js file
                /// throws a JSException if application is offline.
                if (ex.GetType().ToString()
                        .Contains("JSException"))
                {
                    /// Informs the user that a valid connection to the server
                    /// is required.
                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Info",
                        "You must be online to consume this feature.",
                        SwAlIconType.info);
                }
                else
                {
                    /// Informs the user when an unexpected error occurred in a
                    /// clear and meaningful message.
                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Warning",
                        ex.Message,
                        SwAlIconType.warning);
                }
            }
        }

        /// <summary>
        /// Event handler for the onclick event of bell icon button element
        /// of the 'granted' push notification permission status block.
        /// </summary>
        /// <remarks>
        /// A 'granted' status means that the application user has previously
        /// granted permission to receive push notifications. Therefore, if
        /// the 'onclick' event of the bell icon is raised, it means that the
        /// application user requested to be unsubscribed to the push service
        /// of the user agent.
        /// </remarks>
        /// <returns>An asynchronous operation. If successful, it includes
        /// notifying the component that its state has changed to trigger a
        /// re-render.</returns>
        private async Task UnsubscribeFromPushNotificationsAsync()
        {
            try
            {
                /// Unsubscribes the current user from the push service of the user
                /// agent (a computer program representing a person; e.g., a web
                /// browser). Returns the subscription details as a .Net
                /// PushSubscriptionDetails instance with the PushEndpointUrl
                /// as a string and the P256dh and Auth keys as strings encoded
                /// in Base64 format. 
                ///
                /// If application is offline, it throws a JSException from the
                /// Application/Client/wwwroot/js push-notification.js file.
                PushSubscriptionDetails? subscriptionDetails =
                    await JsRuntime.UnsubscribeUserFromPushNotificationsAsync();

                if (subscriptionDetails != null)
                {
                    /// Removes the push subscription details from the database.
                    ///
                    /// The DeletePushSubscriptionAsync method of the
                    /// <see cref="ApiPushSubscriptions"/> class encapsulates
                    /// an <see cref="ApiConnector"/>.InvokePostAsync resource
                    /// method that does not include a security JWT eventhough
                    /// it is a POST action. 
                    ///
                    /// It builds a POST action because we need to pass the
                    /// object value (<see cref="PushSubscriptionDetails"/>) in
                    /// the Http request and resource method InvokeDeleteAsync
                    /// does not have an overload to satisfy that requirement.
                    /// It is a POST operation even though it deletes a record
                    /// from the database.
                    await ApiService.PushSubscriptions
                        .DeletePushSubscriptionAsync(subscriptionDetails);

                    /// Updates the flag responsible for capturing the current
                    /// status for the notifications permission.
                    _pushNotificationPermission =
                        await JsRuntime.GetStatusNotificationPermissionAsync();

                    /// Informs the user that the operation was completed
                    /// successfully.
                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Info",
                        "Successfully unsubscribed from web push notifications.",
                        SwAlIconType.success);

                    /// Re-renders the component to update any recently assigned
                    /// parameter values.
                    StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                ExceptionLoggers.ExtractAndDisplayException(ex);

                /// unsubscribeUserToPushNotifications() function in
                /// Application/Client/wwwroot/js push-notification.js file
                /// throws a JSException if application is offline.
                if (ex.GetType().ToString()
                    .Contains("JSException"))
                {
                    /// Informs the user that a valid connection to the server
                    /// is required.
                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Info",
                        "You must be online to consume this feature.",
                        SwAlIconType.info);
                }
                else
                {
                    /// Informs the user when an unexpected error occurred in a
                    /// clear and meaningful message.
                    await JsRuntime.SwAlDisplayMessageAsync(
                        "Warning",
                        ex.Message,
                        SwAlIconType.warning);
                }
            }
        }
    }
}