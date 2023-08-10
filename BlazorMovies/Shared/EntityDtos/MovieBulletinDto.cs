using BlazorMovies.Shared.EDM;

namespace BlazorMovies.Shared.EntityDtos
{
    /// <summary>
    /// A type that takes a combination of properties
    /// of a type Movie and turns them into a single
    /// <em>model</em> that includes its related data
    /// (entities).
    /// </summary>
    /// <remarks>
    /// Its <em>flattening</em> process provides optimization
    /// for data transfer, prevents sending sensitive info
    /// to the client, and facilitates accessing specific
    /// data of an object of type Movie.
    /// </remarks>
    public class MovieBulletinDto
    {
        /// <summary>
        /// The Movie item (principal entity). Does not hold
        /// any related data. 
        /// </summary>
        public Movie? Movie { get; set; }

        /// <summary>
        /// A collection of items of type Genre that are
        /// related to the Movie item. Does not hold any
        /// related data.
        /// </summary>
        public List<Genre>? Genres { get; set; }
        
        /// <summary>
        /// A collection of items of type Person that are
        /// related to the Movie item. If applicable, it
        /// includes the MovieCharacter.CharacterName that
        /// the actor interpreted in the current Movie object.
        /// </summary>
        public List<Person>? Actors { get; set; }

        /// <summary>
        /// The average of the <see cref="MovieScore"/> records in the data
        /// store for the current <see cref="EDM.Movie"/>
        /// object.
        /// </summary>
        public double MovieScoreAverage { get; set; }

        /// <summary>
        /// The score value selected by the application user for the current
        /// <see cref="EDM.Movie"/> object.
        /// </summary>
        public int UserScore { get; set; }
    }
}

