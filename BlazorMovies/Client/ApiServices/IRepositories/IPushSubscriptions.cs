using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Client.ApiServices.IRepositories
{
    /// <summary>
    /// Entry point for the IApiService and IUnitOfWork interfaces because they
    /// expose one IEntityName interface for each data entity type in the Entity
    /// Domain Model (EDM).
    /// </summary>
    /// <remarks>
    /// <see cref="IPushSubscriptions"/> implements the
    /// <see cref="IRepository{TEntity}"/> interface which has general
    /// functionality applicable to all data entities. It also extends the
    /// general functionality operations that are specific to the entity type
    /// passed to satisfy its type parameter.
    /// <para>
    /// Anything related to 'eager loading' and 'explicit loading' belongs here;
    /// e.g., include related entities (and its property values) in the result
    /// of a query with EF's "Include" extension method.
    /// </para>
    /// </remarks>
    public interface IPushSubscriptions : IRepository<PushSubscriptionDetails>
    {
         #region Post-Create methods

        /// <summary>
        /// Persists into the PushSubscriptions database table a record
        /// with the required data to target a specific end user to send a push
        /// notification. 
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
        /// <see cref="AddPushSubscriptionAsync"/> to persist the
        /// related data into the PushSubscriptions database table.
        /// </para>
        /// </remarks>
        /// <param name="pushSubscriptionDetails">The object that wraps the
        /// data required to target a specific end user to send a push
        /// notification.</param>
        /// <returns>The data entity inserted into the database table.
        /// </returns>
        Task<PushSubscriptionDetails> AddPushSubscriptionAsync(
            PushSubscriptionDetails? pushSubscriptionDetails);

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Employs the Application/Server-Api/Helpers VapidOptions
        /// custom class to retrieve the <em>public key</em> of the VAPID
        /// details.
        /// </summary>
        /// <remarks>
        /// Voluntary Application Server Identity (VAPID) is a way to send and
        /// receive website push notifications. The Application/Server-Api
        /// voluntarily identifies itself to the push service using VAPID.
        /// <para>
        /// An alternative to this method consumed by the GetVapidPublicKeyTask
        /// <dfn>action</dfn> controller is to create a minimal API. Refer to
        /// "Episode 154. Push API - Backend" of Udemy course
        /// <see href="https://www.udemy.com/share/101ZK23@FHXR8HROjz7zUmlmb0LdPBR7qBb3ffpRwyKadwoOp833xq3Gp8pRfnTVDJ6xaO9s/">
        /// Programando en Blazor - ASP.Net Core 7
        /// </see> by Felipe Gavilán and
        /// <see href="https://learn.microsoft.com/en-us/aspnet/core/tutorials/min-web-api?view=aspnetcore-7.0">
        /// Tutorial: Create a minimal API with ASP.Net Core</see>.
        /// </para>
        /// </remarks>
        /// <returns>A public key required by the push service to validate the
        /// push notification.</returns>
        Task<string> GetVapidPublicKeyAsync();

        #endregion

        #region Put-Update methods

        #endregion

        #region Delete methods

        /// <summary>
        /// Removes from the PushSubscriptions database table a record
        /// with the required data to target a specific end user to send a push
        /// notification.
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
        /// 1. <see cref="PushNotification"/> component uses JSInterop to
        /// unsubscribe the user.
        /// </para>
        /// <para>
        /// 2. <see cref="PushNotification"/> component consumes this
        /// <see cref="DeletePushSubscriptionAsync"/> method to remove the
        /// related data from the PushSubscriptions database table.
        /// </para>
        /// </remarks>
        /// <param name="pushSubscriptionDetails">The object that wraps the
        /// data required to target a specific end user to send a push
        /// notification.</param>
        /// <returns>The deleted entity.</returns>
        Task<PushSubscriptionDetails> DeletePushSubscriptionAsync(
            PushSubscriptionDetails? pushSubscriptionDetails);

        #endregion
    }
}

