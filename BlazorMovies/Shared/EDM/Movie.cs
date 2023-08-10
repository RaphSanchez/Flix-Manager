using System.ComponentModel.DataAnnotations;
using BlazorMovies.Shared.CustomAttributes;
using BlazorMovies.Shared.Resources;

/// <summary>
/// You can specify entity configurations with 'Data Annotations' as
/// attributes on the root entities or you can override the 
/// OnModelCreating(ModelBuilder) method from the derived context
/// class of the DataStore.Ef project. The ModelBuilder fluent
/// API configurations take precedence, allow configuring the database
/// schema from the data persistence layer without modifying the root
/// entities (separation of concerns), and provide more configuration
/// options.
/// https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/data-annotations
/// https://docs.microsoft.com/en-us/ef/core/modeling/
/// 
/// Overriding case-sensitivity in a query via string.ToLower() can have a very significant impact
/// on your application's performance. 
/// https://docs.microsoft.com/en-us/ef/core/miscellaneous/collations-and-case-sensitivity
/// https://docs.microsoft.com/en-us/ef/core/modeling/indexes?tabs=data-annotations
/// https://docs.microsoft.com/en-us/ef/core/performance/efficient-querying#us
/// </summary>
namespace BlazorMovies.Shared.EDM
{
    // Built-in Input components:
    // https://docs.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-5.0#built-in-attributes

    // Data Annotations:
    // https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-5.0

    /// <summary>
    /// This class is part of the Entity Domain Model (EDM) and 
    /// is commonly referred to as the Movie entity or root entity. A
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
    /// app's the "entities" are used as "models" to. 
    /// </p>
    /// <p>m
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
    public class Movie
    {
        #region Fields

        /// <summary>
        /// Private backing field for the Title property. A full
        /// property is required for trimming the value supplied
        /// by the user. Otherwise, data validation considers
        /// empty spaces as characters.
        /// </summary>
        private string? _title = string.Empty;

        #endregion

        #region Properties

        public int Id { get; set; }

        [StringLength(50,
            ErrorMessageResourceType = typeof(MovieFormResources),
            ErrorMessageResourceName = "Title_Validation",
            MinimumLength = 4)]
        public string? Title
        {
            get => _title?.Trim();
            set => _title = value;
        }

        /// Initializing string types to empty string prevents
        /// receiving null reference exceptions.
        [StringLength(200,
            ErrorMessageResourceType = typeof(MovieFormResources),
            ErrorMessageResourceName = "Summary_Validation",
            MinimumLength = 20)]
        public string Summary { get; set; } = string.Empty;

        [Movie_EnsureInTheaters(
            ErrorMessageResourceType = typeof(MovieFormResources),
            ErrorMessageResourceName = "InTheaters_Validation")]
        public bool InTheaters { get; set; }

        public string TrailerUrl { get; set; } = string.Empty;

        /// <summary>
        /// Property nullable to prevent the system from assigning a 01/01-0001
        /// value. Nullable DateTime value defaults to DateTime.Today.
        /// </summary>
        /// <remarks>
        /// <strong>NOTICE:</strong> The date notation for the min and max
        /// range values must adhere to the ISO-8601 notation. Otherwise,
        /// when the culture settings of the operating system where the
        /// application is running differ from the settings of the operating
        /// system of the client (or end user), a System.FormatException
        /// is thrown.
        /// <para>
        /// Alternately, you could define a custom validation attribute to
        /// avoid hard coding range values with a specific string format. 
        /// </para>
        /// <para>
        /// See <see href="https://stackoverflow.com/questions/1406046/data-annotation-ranges-of-dates">
        /// Data Annotations Ranges of Dates
        /// </see>,
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.datetime?view=net-7.0#parsing-datetime-values-from-strings">
        /// Parsing DateTime values from strings
        /// </see>,
        /// <see href="https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.rangeattribute?view=net-6.0">
        /// RangeAttribute Class
        /// </see>, and
        /// <see href="https://stackoverflow.com/questions/8844747/restrict-datetime-value-with-data-annotations">
        /// Restrict DateTime value with data annotations</see>.
        /// </para>
        /// </remarks>
        [Range(typeof(DateTime), "2000/1/1", "2030/12/31",
            ErrorMessageResourceType = typeof(MovieFormResources),
            ErrorMessageResourceName = "ReleaseDate_Validation")]
        public DateTime? ReleaseDate { get; set; }

        public string PosterPath { get; set; } = string.Empty;

        public string? TitleSummary
        {
            get
            {
                if (string.IsNullOrEmpty(Title))
                    return null;

                if (Title.Length <= 17) return Title;

                string[] words = Title.Split(' ');
                int characterCount = 0;
                List<string> summary = new();
                foreach (string word in words)
                {
                    characterCount += word.Length + 1;
                    if (characterCount > 17)
                        break;
                    summary.Add(word);
                }

                return string.Join(" ", summary) + "...";
            }
        }

        #endregion

        public Movie()
        {
            /// Instantiation of navigation properties to prevent
            /// a NullReferenceException. 
            Genres = new List<Genre>();
            Actors = new List<Person>();
            MovieCharacters = new List<MovieCharacter>();
        }

        #region Navigation Properties

        /// Navigation properties are nullable (optional) and have a type
        /// ICollection that enables the system to decide the best type
        /// of collection depending of whether it is in the database,
        /// in-memory, or elsewhere.
        /// https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a
        /// https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references#nullable-contexts
        /// https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
#nullable enable
        public ICollection<Genre>? Genres { get; set; }

        public ICollection<Person>? Actors { get; set; }

        /// <summary>
        /// Collection navigation property. A Movie object has multiple
        /// roles played by multiple Person objects (actors). 
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

        #region Data Validation Logic

        /// <summary>
        /// Ensures that Movie.InTheaters property value is false
        /// when Movie.ReleaseDate has a value that is in the future.
        /// </summary>
        /// <returns>True if the evaluation criteria is met, false if
        /// it is not.</returns>
        public bool ValidateInTheaters()
        {
            if (!ReleaseDate.HasValue) return true;

            if (ReleaseDate.GetValueOrDefault().Date
                > DateTime.Now.Date &&
                InTheaters)
            {
                return false;
            }

            return true;
        }

        #endregion
    }
}


