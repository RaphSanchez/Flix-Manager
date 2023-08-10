
using BlazorMovies.Shared.CustomAttributes;

namespace BlazorMovies.Shared.EDM
{
    /// <summary>
    /// Represents a <em>Join Table</em> that contains common data
    /// between a Person and a Movie object. It is the name of the
    /// character played in a Movie by a Person (actor).
    /// </summary>
    [IsAuditable(IsDeletable = true)]
    public class MovieCharacter
    {
        /// <summary>
        /// Identity property of the MovieCharacter.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the character played by a Person
        /// in a Movie.
        /// </summary>
        public string? CharacterName { get; set; } = string.Empty;

        /// <summary>
        /// The indexed position of this MovieCharacter object
        /// in relation to other MovieCharacter objects in
        /// a Movie type.
        /// </summary>
        /// <remarks>
        /// It is determined with the order of the sequence of
        /// actors (Person) provided by the User for a given
        /// Movie object. 
        /// </remarks>
        public int? DesignatedOrder { get; set; }

        /// <summary>
        /// Foreign key property of the reference object type
        /// Person that this MovieCharacter belongs to.
        /// </summary>
        public int PersonId { get; set; }

        /// <summary>
        /// Reference navigation property. It represents the
        /// reference object that this MovieCharacter belongs to.
        /// </summary>
        public Person? Person { get; set; }

        /// <summary>
        /// Foreign key property of the reference object type
        /// Movie that this MovieCharacter belongs to.
        /// </summary>
        public int MovieId { get; set; }

        /// <summary>
        /// Reference navigation property. It represents the
        /// reference object that this MovieCharacter belongs to.
        /// </summary>
        /// <remarks>
        /// A new and unique MovieCharacter object for
        /// each Movie. Even in a sequel, the MovieCharacter
        /// name could be the same but the Person (actor)
        /// could be different and the Movie title is
        /// different. 
        /// </remarks>
        public Movie? Movie { get; set; }
    }
}


