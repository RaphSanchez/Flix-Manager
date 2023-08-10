using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.DataStore;
using BlazorMovies.Server.Helpers;
using BlazorMovies.Shared.EDM;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.Options;

namespace BlazorMovies.Server.Repositories
{
    /// <summary>
    /// One application specific EfEntityName class for each 
    /// IEntityName interface exposed in the IUnitOfWork interface.
    /// </summary>
    /// <remarks>
    /// It is a subclass of the <see cref="EfRepository{TEntity}"/> class
    /// which means it inherits its general functionality applicable to all
    /// entities. 
    /// <para>
    /// This class is application specific and extends its base class
    /// with specific functionality for the type passed as type parameter.
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in 
    /// the result of a query with EF's "Include" extension method.
    /// </para>
    /// <para>
    /// Its "<c>internal</c>" access modifier makes it available only to
    /// elements that reside in the same assembly (project):
    /// Application/Server-Api
    /// </para>
    /// <para>
    /// Its methods have an "explicit interface implementation" to hide
    /// them from unwanted consumers. 
    /// </para>
    /// <para>
    /// It does not have an exception handling mechanism (try-catch blocks)
    /// because exceptions propagate up the stack until a catch statement for
    /// the exception is found. The Application/Server-Api/Controllers
    /// controller that calls method(s) in this repository has an exception
    /// handling mechanism. 
    /// </para>
    /// </remarks>
    internal class EfPushSubscriptions
        : EfRepository<PushSubscriptionDetails>,
            IPushSubscriptions
    {
        /// <summary>
        /// Property designed to have access to the unique DbContext inherited
        /// from its generic Repository parent class. This technique allows you 
        /// to use the property for querying the DB instead of having to make an
        /// explicit conversion on each query (on each method). 
        /// </summary>
        /// <remarks>
        /// <para>
        /// 'Context' is a read-only protected field inherited from the parent
        /// class. This derived class employs its base class's DbContext
        /// instance as opposed to storing its constructor argument in a local
        /// variable to consume it. 
        /// </para>
        /// <para>
        /// Every EfEntityName class that derives from the generic
        /// <see cref="EfRepository{TEntity}"/> class employs the base class's
        /// DbContext protected field which is unique and represents a single
        /// session with the database that can have multiple operations with
        /// different entity types in a single business transaction. 
        /// </para>
        /// </remarks>
        private AppDbContext? AppContext => Context as AppDbContext;

        /// <summary>
        /// VAPID details required to configure the
        /// <see cref="PushNotificationsService"/>. They have been safely
        /// stored in App Secrets using the Secret Manager.
        /// </summary>
        private readonly VapidOptions _vapidOptions;

        public EfPushSubscriptions(
            DbContext context,
            IHttpContextAccessor httpContextAccessor,
            IOptions<VapidOptions> optionsAccessor)
            : base(context, httpContextAccessor)
        {
            _vapidOptions = optionsAccessor.Value;
        }

        #region Post-Create methods

        /// <summary>
        /// Persists into the PushSubscriptionsDetails database table a record
        /// with the required data to target a specific end user to send a push
        /// notification. 
        /// </summary>
        /// <remarks>
        /// Subscribing an application user to the push service of the web
        /// browser is a two step process:
        /// <para>
        /// 1. <see cref="PushNotification"/> component uses JSInterop to
        /// subscribe the user.
        /// </para>
        /// <para>
        /// 2. <see cref="PushNotification"/> component consumes this
        /// <see cref="IPushSubscriptions.AddPushSubscriptionAsync"/>
        /// to persist the related data into the PushSubscriptionsMetadata
        /// database table.
        /// </para>
        /// </remarks>
        /// <param name="pushSubscriptionDetails">The object that wraps the
        /// data required to target a specific end user to send a push
        /// notification.</param>
        /// <returns>The data entity successfully inserted into the database
        /// table.</returns>
        async Task<PushSubscriptionDetails>
            IPushSubscriptions.AddPushSubscriptionAsync(
            PushSubscriptionDetails? pushSubscriptionDetails)
        {
            /// Begins tracking the given entity in the EntityState.Added state
            /// such that it will be inserted into the database when
            /// DbContext.SaveChanges() is called (from the controller at the
            /// end of the business transaction).
            ///
            /// Returns an EntityEntry that provides access to change tracking
            /// information.
            /// https://docs.microsoft.com/en-us/ef/core/change-tracking/
            EntityEntry<PushSubscriptionDetails> dbPushSubscriptionEntry =
                await AppContext!.PushSubscriptionsDetails!
                    .AddAsync(pushSubscriptionDetails!);

            /// Gets the PushSubscription entity being tracked by the
            /// EntityEntry.
            PushSubscriptionDetails insertedPushSubscriptionDetails =
                dbPushSubscriptionEntry.Entity;

            return insertedPushSubscriptionDetails;
        }

        #endregion

        #region Get-Read methods

        /// <summary>
        /// Employs the <see cref="VapidOptions"/> custom class to retrieve
        /// the <em>public key</em> of the VAPID details.
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
        async Task<string> IPushSubscriptions.GetVapidPublicKeyAsync()
        {
            /// Retrieves the public key from the application secrets.
            string vapidPublicKey =
                await Task.FromResult(_vapidOptions.VapidPublicKey);

            return vapidPublicKey;
        }

        #endregion

        #region Put-Update methods

        #endregion

        #region Delete methods

        /// <summary>
        /// Removes from the PushSubscriptionsDetails database table a record
        /// with the required data to target a specific end user to send a push
        /// notification.
        /// </summary>
        /// <remarks>
        /// Unsubscribing an application user from the push service of the web
        /// browser is a two step process:
        /// <para>
        /// 1. <see cref="PushNotification"/> component uses JSInterop to
        /// unsubscribe the user.
        /// </para>
        /// <para>
        /// 2. <see cref="PushNotification"/> component consumes this
        /// <see cref="IPushSubscriptions.DeletePushSubscriptionAsync"/>
        /// to remove the related data from the PushSubscriptionsDetails database
        /// table.
        /// </para>
        /// </remarks>
        /// <param name="pushSubscriptionDetails">The object that wraps the
        /// data required to target a specific end user to send a push
        /// notification.</param>
        /// <returns>The data entity successfully removed from the database
        /// table.</returns>
        async Task<PushSubscriptionDetails>
            IPushSubscriptions.DeletePushSubscriptionAsync(
                PushSubscriptionDetails? pushSubscriptionDetails)
        {
            /// Attempts to retrieve from the database a record that matches
            /// the member values of the pushSubscriptionDetails object passed
            /// by the Application/Client to satisfy the formal input paramter.
            ///
            /// Returns null if not found.
            PushSubscriptionDetails? dbPushSubscription =
                await AppContext?.PushSubscriptionsDetails!
                    .FirstOrDefaultAsync(ps =>
                        ps.P256dh == pushSubscriptionDetails!.P256dh
                        && ps.Auth == pushSubscriptionDetails.Auth)!;

            if (dbPushSubscription == null)
                return null!;

            /// Begins tracking the given entity in the EntityState.Deleted
            /// state such that it will be removed from the database when
            /// DbContext.SaveChanges() is called (from the controller at the
            /// end of the business transaction).
            EntityEntry<PushSubscriptionDetails> dbPushSubscriptionEntry =
                AppContext.PushSubscriptionsDetails?
                    .Remove(dbPushSubscription)!;

            /// Gets the PushSubscription entity being tracked by the
            /// EntityEntry.
            PushSubscriptionDetails deletedPushSubscriptionDetails =
                dbPushSubscriptionEntry.Entity;

            /// Returns the entity being tracked in EntityState.Deleted state.
            return deletedPushSubscriptionDetails;
        }

        #endregion
    }
}


