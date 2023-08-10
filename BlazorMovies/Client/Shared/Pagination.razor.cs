using Microsoft.AspNetCore.Components;

namespace BlazorMovies.Client.Shared
{
    /// <summary>
    /// Pagination component presents data to the client in small and manageable
    /// portions (or segments) through button elements with page numbers that
    /// evenly distribute the full set of available data.
    /// </summary>
    public partial class Pagination
    {
        /// <summary>
        /// Collection of page number button elements available to the client
        /// in the pagination control. Its content (number of button elements)
        /// depends on the pagination <see cref="Radius"/> property value. 
        /// </summary>
        private List<PaginationButtonModel>? _paginationButtons;

        /// <summary>
        /// The page number that corresponds to the records currently displayed
        /// to the user.
        /// </summary>
        [Parameter]
        public int CurrentPage { get; set; }

        /// <summary>
        /// The total number of pages (or segments) that evenly distribute
        /// the total number of records available in the database attending
        /// the specifications outlined in the PaginationRequestDto (e.g.,
        /// records per page).
        /// </summary>
        [Parameter]
        public int TotalPages { get; set; }

        /// <summary>
        /// The total number of button elements to display before and  after
        /// the active page. Does not include "previous" or "next" buttons.
        /// </summary>
        [Parameter]
        public int Radius { get; set; } = 1;

        /// <summary>
        /// Generic event handler (delegate) allows
        /// to pass functionality from the parent component (consumer) as a
        /// parameter to the <see cref="Pagination"/> component. When the
        /// event handler delegate is invoked, it captures the button element
        /// (page number) selected by the user and dispatches an event
        /// notification to the parent component's event handler
        /// (<strong><em>method</em></strong>) which includes the page number
        /// value selected by the user.
        /// </summary>
        /// <remarks>
        /// The notification is sent from the <see cref="OnPageSelected"/>
        /// handler after the button element selected by the user is validated.
        /// <para>
        /// The type parameter <paramref name="int"/> is the page number selected
        /// by the user.
        /// </para>
        /// </remarks>
        [Parameter]
        public EventCallback<int> OnSelectedPageValidated { get; set; }

        /// <summary>
        /// Method invoked when the component has received its parameters from
        /// its parent (consumer) component and the incoming values have been
        /// assigned to properties.
        /// </summary>
        protected override void OnParametersSet()
        {
            /// Produces the button elements of the pagination component.
            CreateButtonElements();
        }

        /// <summary>
        /// Produces the button elements of the pagination component. It
        /// includes the "previous", "next", and page number button controls.
        /// </summary>
        /// <remarks>
        /// The quantity of page number button controls is dependent on the
        /// number of <see cref="TotalPages"/> that evenly distribute the full
        /// set of data and the value passed to satisfy the pagination
        /// <see cref="Radius"/> parameter.
        /// </remarks>
        private void CreateButtonElements()
        {
            /// Instantiation of a new collection of button controls takes
            /// place every time this <see cref="Pagination"/> component
            /// receives new parameters from its parent component (consumer);
            /// e.g., during initialization of the component and each time the
            /// user selects a new page (button element) from the parent
            /// component.
            _paginationButtons = new List<PaginationButtonModel>();

            /// Produces an object of type <see cref="PaginationButtonModel"/>
            /// (button element) that represents the items that belong to the
            /// segment of data of the previous page number using as a reference
            /// the currently active page number (considering the segment of
            /// data currently displayed to the user).
            int previousPageNumber = CurrentPage - 1;
            bool previousPageLinkIsEnabled = CurrentPage > 1;
            string previousButtonText = "Previous";

            _paginationButtons.Add(
                new PaginationButtonModel
                (
                    previousPageNumber,
                    previousPageLinkIsEnabled,
                    previousButtonText
                ));

            /// Produces one or more objects of type
            /// <see cref="PaginationButtonModel"/> (button elements) with
            /// page numbers as text. It relies on the number of TotalPages
            /// required to evenly distribute the full set of data and the
            /// value passed to the Radius parameter to determine the quantity
            /// of button elements to display.
            /// 
            /// It is also responsible for indicating the current
            /// page  number displayed (active page).
            for (int i = 1; i <= TotalPages; i++)
            {
                /// The value for the iteration variable <paramref name="i"/>
                /// represents a page number (or segment of data). If its
                /// value is within range, then a button element is produced
                /// using this value. 
                ///
                /// If the CurrentPage property value is equal to the value of
                /// the iteration variable (or page number of the current
                /// loop), the value for the property named "Active" of the
                /// button element produced is set to "true". This means it is
                /// the "active" page.
                if (i >= CurrentPage - Radius && i <= CurrentPage + Radius)
                {
                    _paginationButtons.Add(
                        new PaginationButtonModel(i) { Active = CurrentPage == i });
                }
            }

            /// Produces an object of type <see cref="PaginationButtonModel"/>
            /// (butoon element) that represents the items that belong to the
            /// segment of data of the next page number using as a reference
            /// the currently active page number (considering the segment of
            /// data currently displayed to the user). 
            int nextPageNumber = CurrentPage + 1;
            bool nextPageLinkIsEnabled = CurrentPage < TotalPages;
            string nextButtonText = "Next";

            _paginationButtons.Add(
                new PaginationButtonModel
                (
                    nextPageNumber,
                    nextPageLinkIsEnabled,
                    nextButtonText
                ));
        }

        /// <summary>
        /// Event handler for each button element of type
        /// <see cref="PaginationButtonModel"/> in the markup section of the
        /// <see cref="Pagination"/> component.
        /// </summary>
        /// <param name="paginationButton">The button element selected by the
        /// user.</param>
        /// <returns>An asynchronous operation.</returns>
        private async Task OnPageSelected(PaginationButtonModel paginationButton)
        {
            if (paginationButton.PageNumber == CurrentPage)
                return;

            if (!paginationButton.Enabled)
                return;

            /// Overwrites the CurrentPage parameter value with the number of
            /// the page that corresponds to the pagination button selected by
            /// the user. 
            this.CurrentPage = paginationButton.PageNumber;

            /// Dispatches an event notification to the parent component
            /// (consumer) and supplies the page number selected by the user.
            await OnSelectedPageValidated.InvokeAsync(paginationButton.PageNumber);
        }

        /// <summary>
        /// Models the state for a given button element in the pagination
        /// component.
        /// </summary>
        /// <remarks>
        /// Implements constructor overloading to initialize its members. The
        /// keyword <paramref name="this"/> invokes another constructor in the
        /// same object; i.e., it passes execution to the constructor that
        /// matches the formal input parameters that it satisfies.
        /// <para>
        /// The technique implemented here with constructor overloading
        /// <strong><em>sets default values for constructor arguments that
        /// are omitted by the consumer.</em></strong>
        /// </para>
        /// </remarks>
        class PaginationButtonModel
        {
            /// <summary>
            /// If an object of type PaginationButtonModel is initialized with
            /// this constructor; i.e., only its
            /// <paramref name="pageNumber"/> parameter is
            /// satisfied, the <paramref name="this"/> keyword passes execution
            /// to the constructor that has <paramref name="pageNumber"/> and
            /// <paramref name="enabled"/> as formal input parameters.
            /// </summary>
            /// <remarks>
            /// It sets the <paramref name="enabled"/> parameter value to
            /// <value>"true"</value>; i.e., it defaults the
            /// <paramref name="enabled"/> parameter to "true" before passing
            /// execution to the constructor that matches the new signature.
            /// </remarks>
            /// <param name="pageNumber">The page number to which a button element
            /// is linked to.</param>
            public PaginationButtonModel(int pageNumber)
            : this(pageNumber, true)
            {

            }

            /// <summary>
            /// When this constructor is called, the <paramref name="this"/>
            /// keyword passes execution to the constructor that has
            /// <paramref name="pageNumber"/>, <paramref name="enabled"/>,
            /// and <paramref name="buttonTextToDisplay"/> as formal input
            /// parameters.
            /// </summary>
            /// <remarks>
            /// It sets the <paramref name="buttonTextToDisplay"/> parameter
            /// value; i.e., it defaults its value to
            /// <value>"pageNumber.ToString()"</value> before passing
            /// execution to the constructor that matches the new signature.
            /// </remarks>
            /// <param name="pageNumber">The page number to which a button
            /// element is linked to.</param>
            /// <param name="enabled">Current state for a button element in
            /// the pagination control.</param>
            public PaginationButtonModel(int pageNumber, bool enabled)
            : this(pageNumber, enabled, pageNumber.ToString())
            {

            }

            /// <summary>
            /// Ultimately, this is the constructor that is executed to
            /// instantiate an object of type <see cref="PaginationButtonModel"/>
            /// regardless of the arguments passed by its consumer.
            /// </summary>
            /// <param name="pageNumber">The page number to which a button element
            /// is linked to.</param>
            /// <param name="enabled">Current state for a button element in the
            /// pagination control.</param>
            /// <param name="buttonTextToDisplay">The text that the pagination
            /// button should display; e.g., "Previous", "3", or "Next".</param>
            public PaginationButtonModel(
                int pageNumber,
                bool enabled,
                string buttonTextToDisplay)
            {
                PageNumber = pageNumber;
                Enabled = enabled;
                ButtonTextToDisplay = buttonTextToDisplay;
            }

            /// <summary>
            /// The page number (segment of data) to which a button element is
            /// linked to.
            /// </summary>
            public int PageNumber { get; set; }

            /// <summary>
            /// Current state for a button element in the pagination control.
            /// </summary>
            /// <remarks>
            /// Its value defaults to true with the first constructor
            /// overload if the user omits satisfying the formal input
            /// parameter named "enabled".
            /// </remarks>
            public bool Enabled { get; set; }

            /// <summary>
            /// The text that the pagination button should display; e.g.,
            /// "Previous", "3", or "Next".
            /// </summary>
            /// <remarks>
            /// Its value defaults to "pageNumber.ToString()" with the second
            /// constructor overload if the user omits satisfying the formal
            /// input parameter named "buttonTextToDisplay".
            /// <para>
            /// The property value is used in the markup to define a
            /// "disabled" state for a given pagination button element.
            /// </para>
            /// </remarks>
            public string ButtonTextToDisplay { get; set; }

            /// <summary>
            /// The page with the data segment currently displayed to the
            /// client.
            /// </summary>
            /// <remarks>
            /// The property value is used in the markup to define an "active"
            /// state for a given pagination button element.
            /// </remarks>
            public bool Active { get; set; } = false;
        }
    }
}


