using System.Timers;
using Microsoft.AspNetCore.Components;
using System.Timers;
using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Generic component provides suggestions of available (matching) items
    /// from a collection in the parent (final consumer) component as text
    /// is typed into its &lt;input&gt; element of type text.
    /// </summary>
    /// <remarks>
    /// It invokes the search functionality from a search method that resides
    /// in the final consumer and notifies its final consumer when the user
    /// selects an item (a suggestion).
    /// <para>
    /// It is consumed by the <see cref="TypeaheadDragSelect{TSelectedItem}"/>
    /// component.
    /// </para>
    /// <para>
    /// The actual type (class or struct) that the
    /// <typeparamref name="TItem"/> represents must implement
    /// a robust value equality comparison; i.e., it must implement the
    /// IEquatable&lt;T&gt; interface to override its "Equals" method.
    /// </para>
    /// </remarks>
    /// <typeparam name="TItem">The type of the collection of items to search
    /// from to find a match. E.g., <see cref="Person"/> items. </typeparam>
    public partial class Typeahead<TItem> : IDisposable
    {
        /// <summary>
        /// Private backing field for the SearchText property
        /// bound to the HTML input element.
        /// </summary>
        private string _searchText = string.Empty;

        /// <summary>
        /// Flag to monitor when the local SearchAsync() method
        /// is called which in turn invokes the SearchMethod
        /// passed as an argument by the parent (consumer) to
        /// specify how and what to obtain from a collection of
        /// options that resides in the parent component.
        /// </summary>
        /// <remarks>
        /// During a search operation, this _isSearching flag is
        /// set to true and the _showSuggestions flag is set to
        /// false because no suggestions are shown while the
        /// SearchAsync() function executes.
        /// </remarks>
        private bool _isSearching = false;

        /// <summary>
        /// Flag to monitor when the search invoked by the
        /// SearchAsync() is completed.
        /// </summary>
        /// <remaks>
        /// When the search is completed, the _isSearching flag
        /// is set to false and the _showSuggestions flag is set
        /// to true.
        /// </remaks>
        private bool _showSuggestions = false;

        /// <summary>
        /// Flag to monitor if and when the mouse pointer is
        /// over a "suggestion" item. If so, it bypasses the
        /// HideSuggestions method. Otherwise, the suggestions
        /// disappear and user cannot select a suggestion item.
        /// </summary>
        private bool _pointerOverSuggestion = false;

        /// <summary>
        /// Collection of suggestions to display for the user.
        /// It is obtained when the local SearchAsync method is
        /// called which in turn invokes the SearchMethod passed
        /// by the parent (consumer) as an argument to specify
        /// what and how to search from.
        /// </summary>
        protected TItem[] Suggestions = Array.Empty<TItem>();

        /// <summary>
        /// Raises the SearchAsync local method which in turn invokes
        /// the parent's (consumer) search method every time its
        /// interval elapses. The timer is reset (stop-start) every
        /// time the user performs a keystroke while the HTML input
        /// element has focus.
        /// </summary>
        private System.Timers.Timer _debounceTimer = null!;

        /// <summary>
        /// Muted text to let the user know what is expected
        /// in the input element.
        /// </summary>
        [Parameter]
        public string Placeholder { get; set; } = null!;

        /// <summary>
        /// Represents a search function that the consumer (parent) component
        /// should define because the Typeahead component does not know the
        /// type of the collection to look from and what to look for; e.g.,
        /// <see cref="Person"/>.Name or <see cref="Movie"/>.Genre.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Replaces an EventCallback&lt;string&gt; because callbacks don't
        /// have a return type. This parameter expects an asynchronous method
        /// with an input parameter of type <see cref="string"/> (search text)
        /// and a return type Task&lt;IEnumerable&lt;TItem&gt;&gt;.
        /// </para>
        /// This method is invoked from the local <see cref="SearchAsync"/>
        /// method which is called every time the user types a letter into
        /// the HTML input element because the <see cref="SearchText"/>
        /// property is bound with an "oninput" event.
        /// </remarks>
        [Parameter]
        public Func<string, Task<IEnumerable<TItem>>> SearchMethod { get; set; } = null!;

        /// <summary>
        /// The <see cref="RenderFragment{TItem}"/> creates a delegate that
        /// encapsulates a function with a <typeparamref name="TItem"/>
        /// parameter. When the foreach block (in markup) executes that
        /// delegate, it passes the <typeparamref name="TItem"/> object. This
        /// object is made available via a special variable named
        /// <strong><em>"Context"</em></strong> that the parent component
        /// (consumer) can use to dynamically define how to render each
        /// <typeparamref name="TItem"/> object.
        /// </summary>
        /// <remarks>
        /// In our example, the parent renders the actor's name to the
        /// right of the actor's picture. Actor is type <see cref="Person"/>.
        /// </remarks>
        [Parameter]
        public RenderFragment<TItem> ResultItemTemplate { get; set; } = null!;

        /// <summary>
        /// The parent component determines what to render if no matches
        /// were found after performing a search operation using the text
        /// passed by the user to the input element.
        /// </summary>
        [Parameter]
        public RenderFragment ShowNotFoundTemplate { get; set; } = null!;

        /// <summary>
        /// Time period between keystrokes before initiating a search
        /// operation. Default value is 3 seconds.
        /// </summary>
        [Parameter]
        public int DebounceInterval { get; set; } = 300;

        /// <summary>
        /// The minimum length of the input text before enabling the
        /// _debounceTimer between keystrokes. Default value is 3
        /// characters.
        /// </summary>
        [Parameter]
        public int MinimumTextLength { get; set; } = 1;

        /// <summary>
        /// <see cref="EventCallback{TItem}"/> allows you to pass a method
        /// (functionality) from the parent component as a parameter to this
        /// child component. The type parameter <typeparamref name="TItem"/>
        /// is the item selected by the user from the
        /// <see cref="Suggestions"/> array. The parameter informs the parent
        /// component that an item has been selected.
        /// </summary>
        [Parameter]
        public EventCallback<TItem> OnItemSelected { get; set; }

        /// <summary>
        /// Full property stores the text typed by the user into the
        /// HTML input element. The property and the input element are
        /// bound with an "oninput" event. This means the event is
        /// raised every time the user types a character into the input
        /// element.
        /// </summary>
        /// <remarks>
        /// The <see cref="System.Timers.Timer"/>.Elapsed event is assigned
        /// the local <see cref="SearchAsync"/> method as its event handler
        /// during initialization of the component. This means that every time
        /// the <see cref="DebounceInterval"/> is exhausted, the
        /// <see cref="SearchAsync"/> method is invoked.
        /// </remarks>
        protected string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;

                if (value.Length == 0)
                {
                    /// If user empties input element, Suggestions array
                    /// is also emptied.
                    Suggestions = Array.Empty<TItem>();

                    _showSuggestions = false;

                    _debounceTimer.Stop();
                }
                else if (value.Length >= MinimumTextLength)
                {
                    /// happens every user's keystroke
                    _debounceTimer.Stop();
                    _debounceTimer.Start();
                }
            }
        }

        protected override void OnInitialized()
        {
            _debounceTimer = new();
            _debounceTimer.Interval = DebounceInterval;
            // reset manually inside the SearchText property.
            _debounceTimer.AutoReset = false;
            _debounceTimer.Elapsed += SearchAsync;

            base.OnInitialized();
        }
        
        /// <summary>
        /// Delegate method assigned to the
        /// <see cref="System.Timers.Timer"/>.Elapsed event. It invokes
        /// the <see cref="SearchMethod"/> passed as an argument by the
        /// consumer (parent) to specify how and what to obtain from a
        /// collection of options that resides in the parent component. It
        /// dynamically returns any matching items as the user types text into
        /// the HTML input element.
        /// </summary>
        /// <remarks>
        /// Also handles <see cref="_isSearching"/> and
        /// <see cref="_showSuggestions"/> flags before and after a search
        /// operation takes place.
        /// </remarks>
        protected async void SearchAsync(object? sender, ElapsedEventArgs eEventArgs)
        {
            /// Before a search takes place
            _isSearching = true;
            _showSuggestions = false;
            /// Executes the supplied work item on the associated
            /// renderer's synchronization context. Required because
            /// we are using a SearchMethod.Invoke instead of an
            /// EventCallback.
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-6.0#synchronization-context
            await InvokeAsync(StateHasChanged);

            /// A search takes place.
            /// <remarks>
            /// The Invoke method invokes the method (or constructor)
            /// represented by the current instance (SearchMethod) using
            /// the specified parameters. You are binding its formal
            /// input parameter to a local variable. Later, the parent
            /// component (consumer) satisfies the SearchMethod parameter
            /// but the string value is obtained from this child component.
            /// </remarks>
            Suggestions = (await SearchMethod.Invoke(_searchText))
                .ToArray();

            /// After a search takes place
            _isSearching = false;
            _showSuggestions = true;
            /// Executes the supplied work item on the associated
            /// renderer's synchronization context. Required because
            /// we are using a SearchMethod.Invoke instead of an
            /// EventCallback.
            /// https://docs.microsoft.com/en-us/aspnet/core/blazor/components/?view=aspnetcore-6.0#synchronization-context
            await InvokeAsync(StateHasChanged);
        }

        /// <summary>
        /// Called by a conditional statement from markup section. If returns
        /// true, the suggestions in Suggestions array are rendered to the user.
        /// </summary>
        /// <remarks>
        /// The <see cref="_showSuggestions"/> flag is set to false or true
        /// before and after a search operation inside the
        /// <see cref="SearchAsync"/> method.
        /// </remarks>
        /// <returns>
        /// True if <see cref="Suggestions"/> array contains suggestions and
        /// the <see cref="_showSuggestions"/> flag is set to true. Otherwise
        /// returns false.
        /// </returns>
        private bool ShouldShowSuggestions()
        {
            return Suggestions.Any() && _showSuggestions;
        }

        /// <summary>
        /// Called by a conditional statement from markup section. If returns
        /// true, <see cref="Suggestions"/> array is empty and flag
        /// <see cref="_showSuggestions"/> is true in which case, the user
        /// receives "No Results Found".
        /// </summary>
        /// <returns>
        /// True if <see cref="Suggestions"/> array is empty and the flag
        /// <see cref="_showSuggestions"/> is set to true. Otherwise returns
        /// false.
        /// </returns>
        private bool ShouldShowNotFound()
        {
            return !Suggestions.Any() && _showSuggestions;
        }

        /// <summary>
        /// Event handler for the "onclick" event of the HTML input
        /// element. If the user clicks inside the input element,
        /// the <see cref="_showSuggestions"/> flag is set to true.
        /// </summary>
        private void ShowSuggestions()
        {
            if (Suggestions.Any())
            {
                _showSuggestions = true;
            }
        }

        /// <summary>
        /// Sets the appropriate value for the
        /// <see cref="_pointerOverSuggestion"/> flag to bypass the
        /// <see cref="HideSuggestions"/> method when the user is trying to
        /// select a suggestion from the <see cref="Suggestions"/> array.
        /// </summary>
        private void OnMouseOverSuggestion()
        {
            _pointerOverSuggestion = true;
        }

        /// <summary>
        /// Sets the appropriate value for the
        /// <see cref="_pointerOverSuggestion"/> flag to enforce the
        /// <see cref="HideSuggestions"/> method when the user clicks any
        /// other place except a suggestion item.
        /// </summary>
        private void OnMouseOutSuggestion()
        {
            _pointerOverSuggestion = false;
        }

        /// <summary>
        /// Event handler for the "onfocusout" event of the HTML input
        /// element. If the user clicks outside the input element, the
        /// <see cref="_showSuggestions"/> flag is set to false.
        /// </summary>
        private void HideSuggestions()
        {
            /// If the user clicks outside the input element but
            /// over a suggestion item for selection, the
            /// HideSuggestions() method is bypassed until the
            /// OnItemSelected.InvokeAsync() event callback is
            /// executed.
            if (!_pointerOverSuggestion)
            {
                _showSuggestions = false;

                SearchText = string.Empty;
            }
        }

        /// <summary>
        /// Handler for the "onclick" event of each suggestion item.
        /// Responsible for invoking the parent's (consumer) method passed as
        /// an argument to satisfy the <see cref="SearchMethod"/> parameter.
        /// </summary>
        /// <param name="suggestionSelected">An item selected by the user from
        /// the <see cref="Suggestions"/> presented to the user.</param>
        /// <returns>An asynchronous operation.</returns>
        private async Task SelectItem(TItem suggestionSelected)
        {
            /// Informs the parent (consumer) that an item (suggestion)
            /// has been selected and invokes its search method
            /// functionality.
            await OnItemSelected.InvokeAsync(suggestionSelected);

            /// Used to bypass the HideSuggestions() method.
            _pointerOverSuggestion = false;

            /// Sets the text of the input element to empty which in
            /// turn clears the HTML input element and empties the
            /// Suggestions array.
            SearchText = string.Empty;
        }

        public void Dispose() => _debounceTimer?.Dispose();
    }
}
