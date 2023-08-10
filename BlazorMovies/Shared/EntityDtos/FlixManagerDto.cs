using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Shared.EntityDtos
{
    /// <summary>
    /// Encapsulates two collections of Movie objects required for the
    /// FlixManager routable component. 
    /// </summary>
    /// <remarks>
    /// Encapsulation with a DTO allows multiple data to be included in a
    /// single Http request/response. It also enables to specify the exact
    /// details of the data required (e.g., property members). 
    /// </remarks>
    public class FlixManagerDto
    {
        /// <summary>
        /// Constructor injection of the Movie collections required for
        /// the FlixManager routable component. 
        /// </summary>
        /// <param name="moviesInTheaters">Collection of Movie objects
        /// currently in theaters.</param>
        /// <param name="moviesUpcomingReleases">Collection of Movie objects
        /// that will be released to theaters in the near future.</param>
        public FlixManagerDto(
            List<Movie> moviesInTheaters,
            List<Movie> moviesUpcomingReleases)
        {
            /// Parameter names in the constructor must match with a
            /// property or field on the object (FlixManagerDto).
            /// This means that each name of the formal input parameters
            /// in the constructor must match a property or field name.
            /// The match can be case-insensitive.
            /// 
            /// Otherwise, System.Text.Json serializer won't be able to
            /// bind to an object property or field on deserialization.
            MoviesInTheaters = moviesInTheaters;
            MoviesUpcomingReleases = moviesUpcomingReleases;
        }

        /// <summary>
        /// Collection of Movie objects currently in theaters.
        /// </summary>
        public List<Movie>? MoviesInTheaters { get; set; }

        /// <summary>
        /// Collection of Movie objects that will be released to theaters in
        /// the near future. 
        /// </summary>
        public List<Movie>? MoviesUpcomingReleases { get; set; }
    }
}
