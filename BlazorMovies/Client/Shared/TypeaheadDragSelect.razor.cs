using BlazorMovies.Shared.EDM;

using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Generic component consumes a <see cref="Typeahead{TItem}"/> generic
    /// component to provide suggestions based on text typed by the user. It
    /// displays the items that the user selected from previous suggestions
    /// provided by the <see cref="Typeahead{TItem}"/> component and
    /// implements a "dragging" functionality to allow the user to order
    /// the selected items; i.e., it provides the "select" and "drag"
    /// functionality using the suggestions provided by a
    /// <see cref="Typeahead{TItem}"/> component.
    /// </summary>
    /// <remarks>
    /// The item suggestions provided by the generic
    /// <see cref="Typeahead{TItem}"/> component are based on functionality
    /// defined in the final consumer (ancestor) passed to this
    /// <see cref="TypeaheadDragSelect{TSelectedItem}"/> component in the
    /// form of a <see cref="SearchMethodFunc"/> parameter which in turn
    /// is passed to the <see cref="Typeahead{TItem}"/> component in the form
    /// of another <paramref name="SearchMethod"/> parameter.
    /// <para>
    /// The <paramref name="SearchMethodFunc"/> and the
    /// <paramref name="SearchMethod"/> parameters are Func delegates with signature:
    /// <code>Func&lt;string, Task&lt;IEnumerable&lt;TSelectedItem&gt;&gt;&gt;</code>
    /// </para>
    /// <para>
    /// The actual type (class or struct) that the
    /// <typeparamref name="TSelectedItem"/> represents must implement
    /// a robust value equality comparison; i.e., it must implement the
    /// IEquatable&lt;T&gt; interface to override its "Equals" method.
    /// </para>
    /// </remarks>
    /// <typeparam name="TSelectedItem">The type of the collection of items
    /// to search from to find a match. E.g., <see cref="Person"/> items.
    /// </typeparam>
    public partial class TypeaheadDragSelect<TSelectedItem>
    {
        /// <summary>
        /// Name for the field that the
        /// <see cref="TypeaheadDragSelect{TSelectedItem}"/> component represents;
        /// e.g., Actors.
        /// </summary>
        [Parameter]
        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Represents a search function that the consumer (parent)
        /// component should define because the Typeahead component
        /// does not know the type of the collection to look from
        /// and what to look for; e.g., Person.Name or Movie.Genre.
        /// Replaces an EventCallback of type string because callbacks
        /// don't have a return type. This parameter expects an
        /// asynchronous method with an input parameter of type string
        /// and a return type Task of IEnumerable of type TSelectedItem.
        /// </summary>
        /// <remarks>
        /// This method is invoked from the SearchAsync() method that
        /// resides in the Typeahead component. It is invoked every time
        /// the user types a letter into the HTML input item because the
        /// SearchText property is bound with an "oninput" event.
        /// </remarks>
        [Parameter]
        public Func<string, Task<IEnumerable<TSelectedItem>>> SearchMethodFunc { get; set; } = null!;

        /// Stores any selected items by the user when clicking on an item
        /// "Suggestion" from the Typeahead component.
        [Parameter]
        public List<TSelectedItem> SelectedItems { get; set; } = null!;

        /// <summary>
        /// The RenderFragment<typeparam name="TSelectedItem"></typeparam>
        /// creates a delegate that encapsulates a function with
        /// a TSelectedItem parameter. When the foreach block (in Typeahead
        /// component markup) executes that delegate, it passes
        /// the TSelectedItem object. This object is made available via a
        /// special variable named "Context" that the parent
        /// component (consumer) can use to dynamically define
        /// how to render each TSelectedItem item.
        /// </summary>
        /// <remarks>
        /// In our example, the parent renders the actor's name to the
        /// right of the actor's picture.
        /// </remarks>
        [Parameter]
        public RenderFragment<TSelectedItem> SuggestionTemplate { get; set; } = null!;

        /// <summary>
        /// The parent component determines how and what to render
        /// if no matches were found after performing a search
        /// operation using the text passed by the user to the input
        /// element of the Typeahead component.
        /// </summary>
        [Parameter]
        public RenderFragment NotFoundTemplate { get; set; } = null!;

        /// <summary>
        /// Event delegate handler for the OnItemSelected event callback
        /// parameter of the Typeahead component that this TypeaheadDragSelect
        /// component consumes.
        /// </summary>
        /// <param name="selectedItem">Type parameter passed by the event
        /// callback delegate that this handler is bound to. It is the
        /// item selected by the user from the suggestions generated from
        /// the input text of the Typeahead component.
        /// </param>
        private void AddSelectedItem(TSelectedItem selectedItem)
        {
            /// Prevents adding multiple times the same item into the
            /// SelectedItems collection. Needs overriding the Equals
            /// method in the Model of the type of TSelectedItem (in our example
            /// it is the Person class).
            if (!SelectedItems.Any(sItem => sItem.Equals(selectedItem)))
            {
                SelectedItems.Add(selectedItem);
            }
        }

        /// <summary>
        /// The RenderFragment<typeparam name="TSelectedItem"></typeparam>
        /// creates a delegate that encapsulates a function with
        /// a TSelectedItem parameter. When the foreach block (in markup)
        /// executes the delegate with each iteration, it passes the
        /// TSelectedItem object. This object is made available via a special
        /// variable named "Context" that the parent component (consumer) can
        /// use to dynamically define how to render each TSelectedItem item.
        /// </summary>
        /// <remarks>
        /// In our example, the parent (MovieForm) renders Person objects; i.e.,
        /// it renders Person.Name which is the actor's name.
        /// </remarks>
        [Parameter]
        public RenderFragment<TSelectedItem> SelectedItemTemplate { get; set; } = null!;

        /// <summary>
        /// Handler for the onclick event assigned to each element of the
        /// SelectedItems collection. It removes the selected item.
        /// </summary>
        /// <param name="selectedItem">The element to remove from the
        /// collection.</param>
        private void RemoveSelectedItem(TSelectedItem selectedItem)
        {
            SelectedItems.Remove(selectedItem);
        }

        /// <summary>
        /// Stores a reference to the draggable item of type TSelectedItem.
        /// </summary>
        private TSelectedItem? _draggableItem;

        /// <summary>
        /// Handler for the @ondragstart event delegate. It instantiates
        /// the _draggableItem field.
        /// </summary>
        /// <param name="draggableItem">The item selected by the user to
        /// drag for changing its indexed position in the SelectedItems
        /// collection.</param>
        private void HandleDragStart(TSelectedItem draggableItem)
        {
            _draggableItem = draggableItem;
        }

        /// <summary>
        /// Handler for the @ondragover event delegate. Extracts the indexed
        /// position of the draggable item and uses this element as a reference
        /// to determine the indexed position of the drop target item when the
        /// mouse pointer is over a valid drop target.
        /// </summary>
        /// <param name="dropTargetItem">An element of the SelectedItems
        /// collection. Preferably with a different indexed position than the
        /// _draggableItem.</param>
        private void HandleDragOver(TSelectedItem dropTargetItem)
        {
            if (dropTargetItem != null
                && _draggableItem != null
                && !dropTargetItem.Equals(_draggableItem))
            {
                int draggableItemIndex = SelectedItems.IndexOf(_draggableItem);
                int dropTargetItemIndex = SelectedItems.IndexOf(dropTargetItem);

                /// Exchanges the indexed position of both items
                SelectedItems[draggableItemIndex] = dropTargetItem;
                SelectedItems[dropTargetItemIndex] = _draggableItem;
            }
        }
    }
}
