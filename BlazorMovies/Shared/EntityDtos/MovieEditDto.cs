using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Shared.EntityDtos
{
    /// <summary>
    /// A type that takes a combination of properties of type
    /// Movie and turns them into a single <em>model</em> which
    /// is used to serve an Http request from the client to
    /// populate the fields of the MovieEdit routable component
    /// with a Movie object and its related data entities.
    /// </summary>
    /// <remarks>
    /// Its <em>flattening</em> process provides optimization for
    /// data transfer (eliminates unnecessary data) and makes the
    /// data easily available.
    /// </remarks>
    public class MovieEditDto
    {
        /// <summary>
        /// The data entity of type Movie to encapsulate.
        /// </summary>
        public Movie? Movie { get; set; }

        /// <summary>
        /// Collection of Genre items that are related to the
        /// Movie object.
        /// </summary>
        public List<Genre>? SelectedGenres { get; set; }

        /// <summary>
        /// Collection of Genre items that are available from
        /// the database because they have not been related
        /// to the Movie object.
        /// </summary>
        public List<Genre>? AvailableGenres { get; set; }

        /// <summary>
        /// Collection of Person items that are related to
        /// the Movie object.
        /// </summary>
        /// <remarks>
        /// Person objects are ordered adhering to the
        /// MovieCharacter.Designated order provided by
        /// the user when selecting the actors (Person objects). 
        /// <para>
        /// It includes the character name of the role that
        /// the given actor played in the Movie. It is made
        /// available through the Person.TempCharacterName
        /// property.
        /// </para>
        /// </remarks>
        public List<Person>? Actors { get; set; }
    }
}

