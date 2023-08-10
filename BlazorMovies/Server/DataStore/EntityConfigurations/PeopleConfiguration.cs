using BlazorMovies.Shared.EDM;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BlazorMovies.Server.DataStore.EntityConfigurations
{
    internal class PeopleConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            #region Entity Configurations (key, indices, relationships)

            /// Sets the property(s) that make up the primary key of
            /// this entity.
            builder
                .HasKey(p => p.Id);

            /// Relationships
            /// https://docs.microsoft.com/en-us/ef/core/modeling/relationships?tabs=fluent-api%2Cfluent-api-simple-key%2Csimple-key
            builder
                .HasMany(p => p.Movies)
                .WithMany(m => m.Actors);

            /// <summary>
            /// The Person.TempCharacterName property is not mapped to a
            /// column in the People table of the database because we do
            /// not intend to persist its value.
            /// </summary>
            /// </para>
            /// <see href="https://docs.microsoft.com/en-us/ef/core/modeling/entity-types?tabs=data-annotations">
            /// Excluding types from the model.</see>
            /// <para>
            /// </para>
            /// </remarks>
            builder
                .Ignore(p => p.TempCharacterName);

            #endregion

            #region Property configurations (alphabetical order)

            builder
                .Property(p => p.Name)
                .IsRequired();

            builder
                .Property(p => p.DateOfBirth)
                .IsRequired();              

            builder
                .Property(p => p.Biography)
                .IsRequired();

            #endregion

            /// <remarks>
            /// Populates the database with an initial set of data. It is managed
            /// by "migrations" without even having established a connection to 
            /// the database; i.e., auto-generated values such as the primary key
            /// need to be explicitly assigned even if it is usually auto-generated
            /// by the database framework. 
            /// </remarks>
            #region Data seeding

            /// Data seeding compatible with shadow properties. A new anonymous
            /// type is required to attach the shadow property value (IsDeleted)
            /// at runtime. In other words, simply omit the entity type after the
            /// "new" operator.
            /// https://docs.microsoft.com/en-us/ef/core/modeling/data-seeding
            /// https://stackoverflow.com/questions/61334172/seed-object-with-shadow-properties
            ///
            /// Person.PictureUrl property value is set to string.Empty because
            /// you added an IFileStorageService to store related images for
            /// Person and Movie objects. Seeding a value for this property will
            /// throw an exception when trying to update and/or delete an object. 
            //builder
            //    .HasData(
            //        new
            //        {
            //            Id = 1,
            //            Name = "Margot Robbie",
            //            Biography = "Some biography",
            //            DateOfBirth = new DateTime(1995, 10, 21),
            //            PictureUrl = string.Empty,

            //            CreatedBy = "Administrator",
            //            CreatedOn = DateTime.Now,
            //            UpdatedBy = string.Empty,
            //            UpdatedOn = DateTime.Now,
            //            IsDeleted = false
            //        },
            //        new
            //        {
            //            Id = 2,
            //            Name = "Scarlett Johansson",
            //            Biography = "Some biography",
            //            DateOfBirth = new DateTime(1995, 10, 21),
            //            PictureUrl = string.Empty,

            //            CreatedBy = "Administrator",
            //            CreatedOn = DateTime.Now,
            //            UpdatedBy = string.Empty,
            //            UpdatedOn = DateTime.Now,
            //            IsDeleted = false
            //        },
            //        new
            //        {
            //            Id = 3,
            //            Name = "Sofia Vergara",
            //            Biography = "Some biography",
            //            DateOfBirth = new DateTime(1995, 10, 21),
            //            PictureUrl = string.Empty,

            //            CreatedBy = "Administrator",
            //            CreatedOn = DateTime.Now,
            //            UpdatedBy = string.Empty,
            //            UpdatedOn = DateTime.Now,
            //            IsDeleted = false
            //        },
            //        new
            //        {
            //            Id = 4,
            //            Name = "Beyonce",
            //            Biography = "Some biography",
            //            DateOfBirth = new DateTime(1995, 10, 21),
            //            PictureUrl = string.Empty,

            //            CreatedBy = "Administrator",
            //            CreatedOn = DateTime.Now,
            //            UpdatedBy = string.Empty,
            //            UpdatedOn = DateTime.Now,
            //            IsDeleted = false
            //        });

            #endregion

            #region Shadow Properties

            /// <remarks>
            /// Individual shadow property attachment as opposed to using
            /// the DbContext.OnModelCreating(ModelBuilder) method to
            /// attach the shadow property to all entities in a single step.
            ///
            /// If a property with the same name exists in the entity class,
            /// then it will be added to the model. Otherwise a new shadow
            /// state property will be added. The shadow property values for
            /// new and modified enttities is configured in the override
            /// overload of the DbContext.SaveChanges() method.
            ///
            /// NOTE THAT the application uses a selective audit mechanism
            /// that includes an optional soft-delete mechanism that includes
            /// an optional soft-delete mechanism using a custo IsAudtiable
            /// attribute. The configuration takes place in a centralized
            /// manner overriding the DbContext.OnModelCreating() and
            /// DbContext.SaveChanges() methods.
            /// </remarks>
            //builder
            //    .Property<bool>("IsDeleted");

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

