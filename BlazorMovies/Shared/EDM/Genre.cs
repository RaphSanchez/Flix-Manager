using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using BlazorMovies.Shared.CustomAttributes;

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
    /// <summary>
    /// This class is part of the Entity Domain Model (EDM) and 
    /// is commonly referred to as the Genre entity or root entity. A
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
    public class Genre
    {
        public Genre()
        {
            /// Instantiation of the navigation property to prevent
            /// NullReferenceException. 
            Movies = new List<Movie>();
        }

        public int Id { get; set; }

        [StringLength(20,
            ErrorMessage = "Length for Genre {0} must be between {2} and {1}.",
            MinimumLength = 4)]
        /// Initializing string types to empty string prevents
        /// receiving null reference exceptions.
        public string Name { get; set; } = string.Empty;

        #region Relationships between two entity types
        /// Relationship Convention:
        /// In EF, navigation properties provide a way to navigate 
        /// a relationship between two entity types. Every object 
        /// can have a 'navigation property' for every relationship 
        /// in which it participates. Navigation properties allow 
        /// you to navigate and manage relationships in both 
        /// directions, returning either a reference object or a 
        /// collection. Code first infers relationships based on the
        /// navigation properties defined on your types (entity types).
        /// 
        /// In addition to navigation properties, it is recommended to 
        /// include 'foreign key' properties on the types that represent
        /// dependent objects (one to one relationships). Any property 
        /// with the same data type as the principal primary key property
        /// and a name of type:
        /// "NavigationPropertyNamePrincipalPrimaryKeyPropertyName"
        /// will be identified as a such; e.g., PersonId
        /// 
        /// Code first infers the 'multiplicity' of the relationship based
        /// on the nullability of the foreign key. If the property is 
        /// nullable then the relationship is registered as optional;
        /// otherwise the relationship is registered as required.
        /// https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
        
        /// <summary>
        /// Collection navigation property. Genres has a many to many 
        /// relationship to the Movie entity. A Genre can appear in
        /// many Movies and a Movie can have many genres. 
        ///
        /// To include a navigation property in the result of a query, 
        /// you can use the Microsoft.EntityFrameworkCore 
        /// 'Include<TEntity,TProperty>(IQueryable<TEntity>, 
        /// Expression<Func<TEntity, TProperty>>)' extension method to
        /// specify the related entities to include in the query results.
        /// 
        /// Navigation property is nullable (optional) and has a type
        /// ICollection that enables the system to decide the best type
        /// of collection depending of whether it is in the database,
        /// in-memory, or elsewhere.
        /// https://stackoverflow.com/questions/55492214/the-annotation-for-nullable-reference-types-should-only-be-used-in-code-within-a
        /// https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references#nullable-contexts
        /// </summary>
#nullable enable
        public ICollection<Movie>? Movies { get; set; }
#nullable disable
        #endregion
    }
}

