using BlazorMovies.Client.Shared;

namespace BlazorMovies.Client.Events
{
    /// <summary>
    /// Application state container defines an
    /// <see cref="UpdateNumberOfPendingOperationsAsync"/> event responsible
    /// for signaling (or notifying) the occurrence of an action. In our
    /// scenario, it will be raised when a new record is persisted into an
    /// object store of our custom IndexedDB as a result of attempting to
    /// execute a mutator (POST, PUT, or DELETE) operation while the
    /// application is offline.
    /// </summary>
    /// <remarks>
    /// <see cref="SynchronizationState"/> is a state container class (a
    /// wrapper) that defines the event handler delegate, the event, and the event
    /// publisher method.
    /// <para>
    /// <para>
    /// The component that sends (or raises) the event is called the publisher
    /// or event sender. In our scenario, it will be any component that can
    /// attempt to send and Http request for a mutator operation; e.g.,
    /// GenreCreate or PeopleEdit routable components.
    /// <para>
    /// The component that receives (or handles) the event is called the
    /// subscriber, event handler, or receiver. In our scenario, the handler
    /// is the <see cref="PwaSync"/> component because it performs an action
    /// (updating the number of pending synchronization operations) to handle
    /// the event.
    /// </para>
    /// </para>
    /// See <see href="https://chrissainty.com/3-ways-to-communicate-between-components-in-blazor/">
    /// 3 Ways to Communicate Between Components in Blazor</see>,
    /// Episode 152. Comunicación entre componentes - Borrado en modo offline
    /// of Udemy course <see href="https://www.udemy.com/share/101ZK23@P_z_otCvbswCJciag5NxSmY35j18kowbLOCX8HqFrUZqiNaxg_M3YeBqkKGKTY0v/">
    /// Programando en Blazor - ASP.Net Core 7</see> by Felipe Gavilán, YouTube
    /// video <see href="https://youtu.be/UvFmHWPB70g">
    /// .Net 6 Blazor Component Communication (Parameter, EventCallback, &amp;
    /// State/Service)</see> by Patrick God, and YouTube video <see href="https://youtu.be/5RpekEKW6E0">
    /// Blazor .Net 6 - Custom Events - Multimedia Paste (Like in Twitter)</see>
    /// by Felipe Gavilán.
    /// </para>
    /// <para>
    /// Any component that subscribes to this event must implement IDisposable
    /// interface to unsubscribe from the event when the component is disposed
    /// of. For more info refer to 
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-7.0">
    /// In-memory state container service
    /// </see> and
    /// <see href="https://learn.microsoft.com/en-us/aspnet/core/blazor/components/lifecycle?view=aspnetcore-7.0#component-disposal-with-idisposable-and-iasyncdisposable">
    /// Component disposal with IDisposable and IAsyncDisposable</see>.
    /// </para>
    /// </remarks>
    public class SynchronizationState : ISynchronizationState
    {
        /// <summary>
        /// Custom event with a <see cref="Func{TResult}"/> event handler
        /// delegate that is the link between the publisher method raising the
        /// event and the subscribers receiving the notification. 
        /// </summary>
        /// <remarks>
        /// The event handler delegate is a pointer or reference to any
        /// compatible event handler method that each subscriber class must
        /// contain. The signature of the event handler delegate must be
        /// compatible with the event handler method, including its return
        /// type.  
        /// </remarks>
        public event Func<Task>? UpdateNumberOfPendingOperationsAsync;

        /// <summary>
        /// Event publisher method responsible for raising (or publishing) the
        /// event. It checks if it has any subscribers in its invocation list,
        /// if so, it invokes its event handler delegate (or method pointer).
        /// </summary>
        /// <returns>An asynchronous operation.</returns>
        public async Task PublishUpdateNumberOfPendingOperationsAsync()
        {
            await UpdateNumberOfPendingOperationsAsync?.Invoke()!;
        }
    }
}


