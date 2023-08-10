
namespace BlazorMovies.Shared.QueryFilterDtos
{
    /// <summary>
    /// Encapsulates property values that can be directly related to
    /// one or more properties of a type Person.
    /// </summary>
    /// <remarks>
    /// The DTO is used by the 
    /// Application/Server-Api/Controllers/PeopleController/FilterPeople
    /// endpoint to retrieve Person items from the database that match
    /// the filtering criteria. 
    /// <para>
    /// <strong>All its members are nullable (optional)</strong> to 
    /// provide more flexibility to the user on the filtering criteria.
    /// </para>
    /// </remarks>
    public class PeopleQueryFilterDto
    {
        /// Parameterless constructor required to allow this "Model bound"
        /// complex type to have "null" values by default. Otherwise, a
        /// System.InvalidOperationException is produced. 
        /// 
        /// An alternative is to give the PeopleQueryFilterDto parameters
        /// a non-null default value. 
        public PeopleQueryFilterDto() { }

        /// <summary>
        /// Constructor injection of the property values required for
        /// applying the filtering criteria to a collection of type
        /// Person.
        /// </summary>
        /// <param name="id">The Person.Id</param>
        /// <param name="name">The Person.Name</param>
        /// <param name="movieCharacterName">The Person.MovieCharacterName</param>
        public PeopleQueryFilterDto(
            int? id = null,
            string? name = null,
            string? movieCharacterName = null)
        {
            /// Parameter names in the constructor must match with a
            /// property or field on the object (PeopleQueryFilterDto 
            /// members). This means that each name of the formal input
            /// parameters in the constructor must match a property or 
            /// field name. The match can be case-insensitive.
            /// 
            /// Otherwise, System.Text.Json serializer won't be able to
            /// bind to an object property or field on deserialization.
            Id = id;
            Name = name;
            MovieCharacterName = movieCharacterName;
        }

        /// <summary>
        /// The Person.Id to use for the filtering criteria.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The Person.Name to use for the filtering criteria.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The Person.MovieCharacters.Name to use for the filtering
        /// criteria; i.e., the character name played by an actor to
        /// filter Movie items with that character name.
        /// </summary>
        public string? MovieCharacterName { get; set; }
    }
}




