using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Shared.QueryFilterDtos
{
    /// <summary>
    /// Encapsulates property values that can be directly related to
    /// one or more properties of a type <see cref="Movie"/> and the
    /// parameters to paginate the response.
    /// </summary>
    /// <remarks>
    /// The DTO is used by the 
    /// Server/Controllers/MoviesController/FilterPaginateMoviesTask
    /// endpoint to retrieve Movie items from the database that match
    /// the filtering criteria. It paginates the results following the
    /// pagination parameters specified in the request.
    /// <para>
    /// Except for the <see cref="PaginationPageNumber"/> and
    /// <see cref="PaginationRecordsPerPage"/> members, <strong>all
    /// its members are nullable (optional)</strong> to provide more
    /// flexibility to the user on the filtering criteria.
    /// </para>
    /// <para>
    /// Unless you provide a parameterless constructor, do <strong><em>not
    /// instantiate its members using the constructor</em></strong> because
    /// the <see cref="MoviesQueryFilterDto"/> must be declared as a class
    /// level field and instantiated from multiple sections (e.g.,
    /// OnInitialized life cycle method). 
    /// </para>
    /// </remarks>
    public class MoviesQueryFilterDto
    {
        /// <summary>
        /// Private backing field for the <see cref="UpComingReleases"/>
        /// property.
        /// </summary>
        /// <remarks>
        /// Value assignment modifies null status in case its consumer did not
        /// provide any value. Having a value facilitates data validation for
        /// the full property <see cref="UpComingReleases"/>.
        /// </remarks>
        private bool? _chBoxUpcoming = false;

        /// <summary>
        /// Private backing field for the <see cref="InTheaters"/> property.
        /// </summary>
        /// <remarks>
        /// Value assignment modifies null status in case its consumer did not
        /// provide any value. Having a value facilitates data validation for
        /// the full property <see cref="InTheaters"/>.
        /// </remarks>
        private bool? _chBoxInTheaters = false;

        /// <summary>
        /// The Movie.Id to use for the filtering criteria.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The Movie.Title to use for the filtering criteria.
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// The Movie.Genre to use for the filtering criteria.
        /// </summary>
        public string? Genre { get; set; }

        /// <summary>
        /// Filtering criteria for Movie items that have not been
        /// released in theaters; i.e., for Movie items where their
        /// Movie.ReleaseDate property value is > current date.
        /// </summary>
        /// <remarks>
        /// Prevents the user from selecting both checkboxes:
        /// <see cref="UpComingReleases"/> and <see cref="InTheaters"/>.
        /// </remarks>
        public bool? UpComingReleases
        {
            /// Data validation at the class member level works because
            /// Blazor implements two way data binding which allows
            /// synchronizing a variable (field, property, or Razor
            /// expression value) with an Html element so that any changes
            /// made are reflected on either end of the data. 
            get => _chBoxUpcoming;
            set
            {
                _chBoxUpcoming = value;
                _chBoxInTheaters = _chBoxUpcoming == true 
                    ? false 
                    : _chBoxInTheaters;
            }
        }

        /// <summary>
        /// Filtering criteria based on the Movie.InTheaters property
        /// value.
        /// </summary>
        /// <remarks>
        /// Prevents the user from selecting both checkboxes:
        /// <see cref="UpComingReleases"/> and <see cref="InTheaters"/>.
        /// </remarks>
        public bool? InTheaters
        {
            /// Data validation at the class member level works because
            /// Blazor implements two way data binding which allows
            /// synchronizing a variable (field, property, or Razor
            /// expression value) with an Html element so that any changes
            /// made are reflected on either end of the data. 
            get => _chBoxInTheaters;
            set
            {
                _chBoxInTheaters = value;
                _chBoxUpcoming = _chBoxInTheaters == true 
                    ? false 
                    : _chBoxUpcoming;
            }
        }

        /// <summary>
        /// The Movie.Actor to use for the filtering criteria.
        /// </summary>
        public string? ActorName { get; set; }

        /// <summary>
        /// The Movie.MovieCharacter.Name to use for the filtering
        /// criteria.
        /// </summary>
        public string? CharacterName { get; set; }

        /// <summary>
        /// The page number that corresponds to the records that will be
        /// served.
        /// </summary>
        /// <remarks>
        /// Has a default value because it overwrites the default value
        /// defined for the source type PaginationRequestDto. Otherwise,
        /// if no value is provided, it will default to '0' (zeroe) because
        /// it is a type <see cref="Int32"/>.
        /// </remarks>
        public int PaginationPageNumber { get; set; } = 1;

        /// <summary>
        /// Number of records to include per page.
        /// </summary>
        /// <remarks>
        /// Has a default value because it overwrites the default value
        /// defined for the source type PaginationRequestDto. Otherwise,
        /// if no value is provided, it will default to '0' (zeroe) because
        /// it is a type <see cref="Int32"/>.
        /// </remarks>
        public int PaginationRecordsPerPage { get; set; } = 10;

        /// <summary>
        /// Encapsulates the pagination parameters (which page and how many
        /// records per page) to build an Http request. 
        /// </summary>
        /// <remarks>
        /// This member cannot be assigned to because it is read-only. It
        /// has no <em>setter</em> method. It is built at run-time using the
        /// <see cref="PaginationPageNumber"/> and the
        /// <see cref="PaginationRecordsPerPage"/> member values. 
        /// </remarks>
        public PaginationRequestDto PaginationRequestDto =>
            new()
            {
                PageNumber = PaginationPageNumber,
                RecordsPerPage = PaginationRecordsPerPage
            };
    }
}

