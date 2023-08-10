using BlazorMovies.Shared.EDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

/// <summary>
/// Implement the ModelBuilder fluent API to define configuration
/// settings for the related root entity. Name the configuration 
/// class with a prefix of its related class. 
/// https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.metadata.builders.entitytypebuilder-1?view=efcore-5.0
/// https://docs.microsoft.com/en-us/ef/core/modeling/generated-properties?tabs=fluent-api
/// https://docs.microsoft.com/en-us/sql/t-sql/functions/functions?view=sql-server-ver15
/// https://docs.microsoft.com/en-us/ef/core/
/// </summary>
/// <remarks>
/// Validation errors thrown based on the Fluent API configurations
/// will not automatically reach the UI. However, you can capture them
/// in the Application/Server-Api/Controllers code and then respond to
/// it accordingly.
/// https://docs.microsoft.com/en-us/ef/ef6/saving/validation
/// <p>
/// Or better yet:
/// <see href="https://docs.microsoft.com/en-us/ef/ef6/saving/validation#ivalidatableobject">
/// IValidatableObject Interface.</see> (validation on entity classes-preferred).
/// <br/>
/// <see href="https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.validationattribute?view=net-6.0">ValidationAttribute Class</see>
/// (validation on entity classes decorated with a custom validation attribute).
/// </p>
/// </remarks>
namespace BlazorMovies.Server.DataStore.EntityConfigurations
{
    internal class MovieCharactersConfiguration :
        IEntityTypeConfiguration<MovieCharacter>
    {
        public void Configure(EntityTypeBuilder<MovieCharacter> builder)
        {
            #region Entity Configurations (key, indices, relationships)

            /// Sets the property(s) that make up the primary key of
            /// this entity.
            builder.HasKey(mC => mC.Id);

            /// Relationship convention:
            /// In EF, navigation properties provide a way to navigate a
            /// relationship between two entity types. Navigation properties
            /// navigate and manage relationships in both directions, 
            /// returning either a reference object (single object) or a 
            /// collection. Every relationship must be defined in both ends.
            /// 
            /// In addition to navigation properties, it is recommended to 
            /// include a 'HasForeignKey' method on the types that represent
            /// dependent objects (one to one relationships).
            /// https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
            builder
                .HasOne(mC => mC.Person)
                .WithMany(p => p.MovieCharacters);

                builder
                    .HasOne(mC => mC.Movie)
                    .WithMany(m => m.MovieCharacters);

            #endregion

            #region Property Configurations

            /// By convention, cascade delete will be set to cascade for
            /// required relationships and ClientSetNull for optional
            /// relationships.
            ///
            /// Cascade means dependent entities (such as this type) are
            /// also deleted. ClientSetNull means that dependent entities
            /// that are not loaded into memory will remain unchanged and
            /// must be manually deleted or updated to point to a valid
            /// principal entity. For entitied that are loaded into memory,
            /// EF Core will attempt to set the foreign key properties to
            /// null. You can use the fluent API to modify this behaviour.
            /// https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key#cascade-delete
            builder
                .Property(mC => mC.PersonId)
                .IsRequired();

            builder
                .Property(mC => mC.MovieId)
                .IsRequired();


            #endregion

            #region Global query filters

            /// Linq query predicate (filter) applied automatically to any
            /// Linq query involving this entity type. EF Core also applies
            /// the filter to entity types referenced indirectly through the
            /// use of Include or navigation property.
            ///
            /// This is the heart of the "Soft Delete" mechanism for the
            /// application because any Linq query to this entity will
            /// omit records with an IsDeleted.Property.Value = true.
            ///
            /// Global query filters may be disabled for individual Linq
            /// queries (e.g., in the EntityController by using the
            /// IgnoreQueryFilters operator.
            /// https://docs.microsoft.com/en-us/ef/core/querying/filters
            /// 
            /// Global query filters applied individually to each entity
            /// may be a good idea to fine tune future queries because
            /// each entity could have a different criteria on soft
            /// deleting. Some might require a deactivating-reactivating
            /// behaviour and some might use a soft delete - create new
            /// entity behaviour.
            /// https://spin.atomicobject.com/2019/01/29/entity-framework-core-soft-delete/
            /// 
            /// EF.Property<TProperty>(Object, String) method reference a
            /// given property or navigation on an entity instance. This is
            /// useful for shadow state properties, for which no CLR property
            /// exists.
            /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.ef.property?view=efcore-6.0
            builder.HasQueryFilter(e =>
                EF.Property<bool>(e, "IsDeleted") == false);

            #endregion
        }
    }
}


