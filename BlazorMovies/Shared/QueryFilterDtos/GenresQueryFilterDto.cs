namespace BlazorMovies.Shared.QueryFilterDtos
{
    /// <summary>
    /// Encapsulates property values that can be directly related to
    /// one or more properties of a type Genre.
    /// </summary>
    /// <remarks>
    /// The DTO is used by the 
    /// Application/Server-Api/Controllers/GenresController/FilterGenres
    /// endpoint to retrieve Genre items from the database that match
    /// the filtering criteria. 
    /// <para>
    /// <strong>All its members are nullable (optional)</strong> to 
    /// provide more flexibility to the user on the filtering criteria.
    /// </para>
    /// </remarks>
    public class GenresQueryFilterDto
    {
        /// Parameterless constructor required to allow this "Model bound"
        /// complex type to have "null" values by default. Otherwise, a
        /// System.InvalidOperationException is produced. 
        /// 
        /// An alternative is to give the GenresQueryFilterDto parameters
        /// a non-null default value. 
        public GenresQueryFilterDto() { }

        /// <summary>
        /// Constructor injection of the property values required for
        /// applying the filtering criteria to a collection of type Genre.
        /// </summary>
        /// <param name="id">The Genre.Id</param>
        /// <param name="name">The Genre.Name</param>
        /// <param name="movieTitle">The Genre.Movie.Title</param>
        public GenresQueryFilterDto(
            int? id = null,
            string? name = null)
        {
            /// Parameter names in the constructor must match with a
            /// property or field on the object (GenresQueryFilterDto 
            /// members). This means that each name of the formal input
            /// parameters in the constructor must match a property or 
            /// field name. The match can be case-insensitive.
            /// 
            /// Otherwise, System.Text.Json serializer won't be able to
            /// bind to an object property or field on deserialization.
            Id = id;
            Name = name;
        }

        /// <summary>
        /// The Genre.It to use for the filtering criteria.
        /// </summary>
        public int? Id { get; set; }

        /// <summary>
        /// The Genre.Name to use for the filtering criteria.
        /// </summary>
        public string? Name { get; set; }
    }
}
