using BlazorMovies.Client.Helpers;

using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Custom component presents the user a collection of available options
    /// and allows the selection of one or more items from the collection.
    /// It employs a <see cref="MultipleSelectorDto"/> type which represents
    /// a &lt;Key,Value&gt; pair of &lt;id, string&gt; values; e.g.,
    /// &lt;GenreId., Genre.Name&gt;. 
    /// </summary>
    public partial class MultipleSelector
    {
        /// <summary>
        /// Name for the field that the MultipleSelector component
        /// represents.
        /// </summary>
        [Parameter]
        public string FieldName { get; set; } = null!;

        /// <summary>
        /// Represents a collection of available options; i.e., the ones
        /// that have not been selected. E.g., the full collection of
        /// Key-Value (&lt;Genre.Id, Genre.Name&gt;) pairs available for
        /// selection.
        /// </summary>
        /// <remarks>
        /// The consumer is responsible for mapping the
        /// items in its collection of options to this collection of Key-Value
        /// pairs that can be processed by the <see cref="MultipleSelector"/>
        /// component. 
        /// </remarks>
        [Parameter]
        public List<MultipleSelectorDto> MappedUnSelected { get; set; } = null!;

        /// <summary>
        /// Represents a collection of selected options; i.e., the ones
        /// that have been selected. E.g., the full collection of Key-Value
        /// (&lt;Genre.Id, Genre.Name&gt;) pairs that have been previously
        /// selected. 
        /// </summary>
        /// <remarks>
        /// The consumer is responsible for mapping the
        /// items in its collection of options to this collection of Key-Value
        /// pairs that can be processed by the <see cref="MultipleSelector"/>
        /// component. 
        /// </remarks>
        [Parameter]
        public List<MultipleSelectorDto> MappedSelected { get; set; } = null!;

        /// <summary>
        /// Event handler for each "select-item" button element assigned to
        /// every available option (UnSelected) passed as an argument to
        /// satisfy the MappedUnSelected parameter.
        /// </summary>
        /// <remarks>
        /// It removes the selected item from the UnSelected collection
        /// and it adds the selected item to the Selected collection.
        /// </remarks>
        /// <param name="selectedItem">The item selected by the User.</param>
        protected void SelectItem(MultipleSelectorDto selectedItem)
        {
            MappedUnSelected.Remove(selectedItem);
            MappedSelected.Add(selectedItem);
        }

        /// <summary>
        /// Event handler for each "unselect-item" button element assigned
        /// to every option (Selected) passed as an argument to satisfy the
        /// MappedSelected parameter.
        /// </summary>
        /// <remarks>
        /// It removes the selected item from the Selected collection and
        /// it adds the selected item to the UnSelected collection.
        /// </remarks>
        /// <param name="unSelectedItem">The item selected by the User.</param>
        protected void UnSelectItem(MultipleSelectorDto unSelectedItem)
        {
            MappedSelected.Remove(unSelectedItem);
            MappedUnSelected.Add(unSelectedItem);
        }

        /// <summary>
        /// Event handler for the "Select All" button element removes all the
        /// available options from the collection of Unselected items to the
        /// collection of Selected items.
        /// </summary>
        private void SelectAll()
        {
            MappedSelected.AddRange(MappedUnSelected);
            MappedUnSelected.Clear();
        }

        /// <summary>
        /// Event handler for the "Unselect All" button element removes all
        /// the options selected from the collection of Selected items to the
        /// collection of Unselected items.
        /// </summary>
        private void UnselectAll()
        {
            MappedUnSelected.AddRange(MappedSelected);
            MappedSelected.Clear();
        }
    }
}
