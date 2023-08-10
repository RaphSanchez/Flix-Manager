using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Principal;

using BlazorMovies.Shared.CustomAttributes;

namespace BlazorMovies.Shared.EDM
{
    // Built-in Input components:
    // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-5.0#built-in-attributes
    // Data Annotations:
    // https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-5.0

    /// <summary>
    /// This class is part of the Entity Domain Model (EDM) and 
    /// is commonly referred to as the Person entity or root entity. A
    /// data entity is the .Net object that has a direct relationship
    /// with the database. It is the .Net object representation of
    /// record in a table (class representing the data in a database
    /// table).
    /// </summary>
    /// <remarks>
    /// Implements IActive interface to be included in the Soft Delete
    /// Mechanism.
    /// <para>
    /// The process of flattening takes a combination of one or more
    /// data entities that have a relationship (e.g., Movie with genres)
    /// and of one or more properties of each entity and turns it into
    /// a single "model" (class) which will be used to display or 
    /// render into the browser. These are very similar to Data
    /// Transfer Objects (DTOs) either of which provide optimization
    /// benefits and prevent sending excessive and/or sensitive info
    /// back to the client in the HttpResponse.
    /// </para>
    /// <para>
    /// Classes that communicate to the database are referred to as
    /// <strong>entities</strong> and classes that represent a model
    /// of one or more data entities and their properties are called
    /// <strong>models</strong>; i.e., "models" for MVC (Model View
    /// Controller) and "entities" for entity framework.
    /// </para>
    /// <p>
    /// Commonly you would add a new "project library" (.dll file)
    /// named "Models" to store the model classes and name each
    /// model class with the same name of the primary entity that
    /// it represents and add the "Model" suffix. In small (basic)
    /// app's the "entities" are umsed as "models" to. 
    /// </p>
    /// <p>
    /// Since "model" classes are commonly produced by deriving
    /// them from their related "entity", <strong>entity-model 
    /// validation should be implemented on data entities (server side).
    /// </strong>
    /// </p>
    /// <para>
    /// <see href="https://youtu.be/6145Q1juVHI">
    /// Coding Tutorials: Models vs. Entities</see>
    /// </para>
    /// <para>
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-6.0">
    /// Prevent over-posting</see>
    /// </para>
    /// <para>
    /// <see href="https://docs.microsoft.com/en-us/ef/ef6/saving/validation#ivalidatableobject">
    /// IValidatableObject Interface</see><br/>(custom validation on entity classes preferred).
    /// </para>
    /// <para>
    /// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validationattribute?view=net-6.0">
    /// ValidationAttribute Class</see><br/>(custom validation attributes on entity classes - require decoration).
    /// </para>
    /// <para>
    /// Also refer to Udemy: Complete Web API in .Net 5 episodes 29, 33, 34, 41, 42. 
    /// </para>
    /// </remarks>
    [IsAuditable(IsDeletable = true)]
    public class Person : IEquatable<Person>
    {
        public Person()
        {
            /// Instantiation of the navigation property to prevent
            /// NullReferenceException. 
            Movies = new List<Movie>();

            /// Instantiation of the navigation property to prevent
            /// NullReferenceException. 
            MovieCharacters = new List<MovieCharacter>();
        }

        #region Properties

        [Required]
        public int Id { get; set; }

        [StringLength(50,
            ErrorMessage = "{0} length must be between {2} and {1} characters.",
            MinimumLength = 4)]
        /// Initializing string types to empty string prevents
        /// receiving null reference exceptions.
        public string Name { get; set; } = String.Empty;

        [StringLength(200,
            ErrorMessage = "{0} length must be between {2} and {1} characters.",
            MinimumLength = 15)]
        public string Biography { get; set; } = String.Empty;

        [Required] public string PictureUrl { get; set; } = String.Empty;

        /// <summary>
        /// Property nullable to prevent the system from assigning a 01/01-0001
        /// value. Nullable DateTime value defaults to DateTime.Today.
        /// </summary>
        public DateTime? DateOfBirth { get; set; }

        /// <summary>
        /// Captures a character name for a given Person object (actor)
        /// selected by the user for a Movie object.
        /// </summary>
        /// <remarks>
        /// It is not mapped to a column in the People table of the database
        /// because we do not intend to persist its value; i.e., its
        /// Application/DataStore/PeopleConfiguration file explicitly
        /// specifies an "Ignore" method to exclude this property from the
        /// entity type.
        /// <para>
        /// Its value is wrapped into a MovieEssentialsDto and sent
        /// in the Http request to be handled by the business logic layer of
        /// the application.
        /// </para>
        /// <para>
        /// Its value is also used by the
        /// Application/Repository/EFManager/EfMovies repository to serve
        /// an Http Get request. 
        /// </para>
        /// <para>
        /// <see href="https://docs.microsoft.com/en-us/ef/core/modeling/entity-types?tabs=data-annotations">
        /// Excluding types from the model.</see>
        /// </para>
        /// </remarks>
        public string? TempCharacterName { get; set; } = string.Empty;

        #endregion

        #region Navigation Properties

        /// Navigation property is nullable (optional) and has a type
        /// ICollection that enables the system to decide the best type
        /// of collection depending of whether it is in the database,
        /// in-memory, or elsewhere.
        /// https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a
        /// https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references#nullable-contexts
        /// https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
#nullable enable
        public ICollection<Movie>? Movies { get; set; }

        /// <summary>
        /// Collection navigation property. A Person object (e.g.,
        /// actor) is likely to play multiple roles in multiple movies. 
        /// </summary>
        /// <remarks>
        /// The MovieCharacter type represents a <em>Join Table</em>
        /// that contains common data between a Person and a Movie
        /// object. It is the name of the character played in a
        /// Movie by a Person (actor).
        /// </remarks>
        public ICollection<MovieCharacter>? MovieCharacters { get; set; }
#nullable disable

        #endregion

        #region Interface Implementations

        /// <summary>
        /// Two Person objects have value equality (equivalence) if and only
        /// if they have the same Primary Key (Id). Otherwise they are not
        /// equivalent even if the have the same values (except their Id).
        /// </summary>
        /// <param name="otherPerson">The other person instance to compare
        /// with.</param>
        /// <returns>
        /// True if both person instances share the same Primary
        /// Key (Id). Otherwise false.
        /// </returns>
        public bool Equals(Person otherPerson)
        {
            if (otherPerson is null) return false;
            if (ReferenceEquals(this, otherPerson)) return true;
            return Id == otherPerson.Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Person)obj);
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(Person left, Person right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Person left, Person right)
        {
            return !Equals(left, right);
        }

        #endregion
    }
}