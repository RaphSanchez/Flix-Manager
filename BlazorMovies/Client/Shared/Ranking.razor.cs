using BlazorMovies.Shared.EDM;

using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Uses <see href="https://fontawesome.com/">Font Awesome</see> to create
    /// star elements that represent a <see cref="MovieScore"/> value selected
    /// by the current <see cref="ApplicationUser"/>. It captures the
    /// <see cref="SelectedScore"/> and passes it to its parent component (to
    /// its consumer).
    /// </summary>
    public partial class Ranking
    {
        /// Flag indicates if the application user has selected a star that
        /// represents a <see cref="MovieScore"/> value.
        private bool _userHasVoted = false;

        /// The maximum score value that can be assigned to the rated item;
        /// e.g., 05 stars to a Movie object.
        [Parameter]
        public int MaxScore { get; set; }

        /// The score value selected by a given application user.
        [Parameter]
        public int SelectedScore { get; set; }

        /// Notifies to its consumer (the parent component) when a user has
        /// selected a new <see cref="MovieScore"/> value, captures the value,
        /// and passes it to the parent component. 
        [Parameter]
        public EventCallback<int> OnScoreSelected { get; set; }

        /// Handles the OnClick event for each of the star objects created with
        /// FontAwesome in the 'for' loop of the markup section.
        private async Task OnClickHandler(int starNumber)
        {
            /// Captures the number value of the star selected by the user.
            SelectedScore = starNumber;

            /// Flag indicates that the current user has selected a new value.
            _userHasVoted = true;

            /// Notifies the parent component (consumer) that the user has
            /// selected a score for the current Movie and passes the value of
            /// the selected star.
            await OnScoreSelected.InvokeAsync(SelectedScore);
        }

        /// Handles the OnMouseOver event for each of the star objects created
        /// in the 'for' loop with FontAwesome. It provides a visual feedback
        /// to the user.
        private void OnMouseOverHandler(int starNumber)
        {
            /// If the user has not voted, it captures the index value of the
            /// star that the mouse pointer is hovering and updates the
            /// SelectedRating property.
            ///
            /// The SelectedScore property value is used in the markup section
            /// to dynamically set the CSS class responsible for the fill color
            /// of the star elements. 
            if (!_userHasVoted)
            {
                SelectedScore = starNumber;
            }
        }
    }
}


