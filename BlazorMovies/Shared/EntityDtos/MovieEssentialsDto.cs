using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Shared.EntityDtos
{
    /// <summary>
    /// A type that takes a combination of properties of type Movie
    /// and turns them into a single <em>model</em> which is used to
    /// build and Http request from the client.
    /// </summary>
    /// <remarks>
    /// It includes an array of Movie.Genre Ids and a dictionary with
    /// (Person.Id, Person.CharacterName) as (Key, Value) pairs of the
    /// Person items related to the Movie object.
    /// <para>
    /// Its <em>flattening</em> process provides optimization for data
    /// transfer, multiple data to be included in a single Http request,
    /// and it allows specifying the exact details of the data required
    /// (e.g., property members).
    /// </para>
    /// </remarks>
    public class MovieEssentialsDto
    {
        /// <summary>
        /// Constructor injection of a Movie entity and any related
        /// data represented by its navigation properties.
        /// </summary>
        /// <param name="movie">The data entity of type Movie
        /// to encapsulate.</param>
        /// <param name="relatedGenreIds">An array with the primary
        /// keys of the Genre items related to the Movie entity. 
        /// </param>
        /// <param name="relatedActorsDictionary">A collection with
        /// (Person.Id, Person.TempCharacterName) as (Key,Value)
        /// pairs of the Person items related to the Movie entity.
        /// <remarks>
        /// The Person.TempCharacterName is passed as user input in
        /// the Actors field of the MovieForm component and needs to
        /// be captured to be persisted to the database.
        /// </remarks>
        /// </param>
        public MovieEssentialsDto(
            Movie? movie,
            int[]? relatedGenreIds = null, 
            Dictionary<int, string?>? relatedActorsDictionary = null)
        {
            /// Parameter names in the constructor must match with a
            /// property or field on the object (MovieEssentialsDto).
            /// This means that each name of the formal input parameters
            /// in the constructor must match a property or field name.
            /// The match can be case-insensitive.
            /// 
            /// Otherwise, System.Text.Json serializer won't be able to
            /// bind to an object property or field on deserialization.
            Movie = movie;
            RelatedGenreIds = relatedGenreIds;
            RelatedActorsDictionary = relatedActorsDictionary;
        }

        /// <summary>
        /// The data entity of type Movie to encapsulate.
        /// </summary>
        public Movie? Movie { get; set; }
        
        /// <summary>
        /// Collection of primary keys of related data entities of type
        /// Genre.
        /// </summary>
        public int[]? RelatedGenreIds { get; set; }

        /// <summary>
        /// Dictionary of (Person.Id, Person.CharacterName) as
        /// (Key, Value) pairs of the Person items related to the
        /// Movie entity.
        /// </summary>
        public Dictionary<int, string?>? RelatedActorsDictionary { get; set; }
    }
}

