using System.Reflection;

using BlazorMovies.Shared.CustomAttributes;
using BlazorMovies.Shared.EDM;

using Duende.IdentityServer.EntityFramework.Options;

using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Options;

/// <summary>
/// .Net classes (in the Application/Shared/EDM project) that define
/// the conceptual model become the 'root entities' of the model. You
/// need to expose a DbSet<T> property for the types you want to 
/// include in the model to be mapped to a database table. These
/// DbSet<T>s are automatically initialized when an instance of the 
/// derived context class (AppDbContext) is created. 
/// https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/conventions/built-in
/// https://docs.microsoft.com/en-us/ef/ef6/modeling/code-first/fluent/relationships
/// 
/// The DbSet<T>s are exposed in the "database context" which is
/// the main class that coordinates Entity Framework functionality.
/// 
/// The Application/Server/Program.cs dependency injection container
/// provides the service (an instance of the DbContext derived class)
/// to the IUnitOfWork service (instance of the UnitOfWork class)
/// consumed by the action methods of the
/// Application/Server/Controllers/.
/// https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-6.0&tabs=visual-studio 
/// </summary>
namespace BlazorMovies.Server.DataStore
{
    /// <summary>
    /// Main class that coordinates Entity Framework functionality.
    /// It must be configured as a service in the dependency injection
    /// container of the Application/Server/Startup.cs
    /// </summary>
    /// <remarks>
    /// <see cref="ApiAuthorizationDbContext{TUser}"/> includes the schema for
    /// IdentityServer. It is derived from <see cref="IdentityDbContext"/>.
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0">
    /// ApplicationDbContext</see>.
    /// </para>
    /// </remarks>
    public class AppDbContext : ApiAuthorizationDbContext<ApplicationUser>
    {
        /// <summary>
        /// The name of the current user for the current HttpRequest.
        /// </summary>
        private readonly string? _userName;

        /// <summary>
        /// Injects a dependency to an IHttpContextAccessor that
        /// provides access to the current HttpContext information and
        /// configures options for the IdentityServer engine operational
        /// data.
        /// </summary>
        /// <remarks>
        /// See <see href="https://docs.duendesoftware.com/identityserver/v6/quickstarts/4_ef/">
        /// Using EF Core for configuration and
        /// operational data.</see>
        /// </remarks>
        /// <param name="options">Any DbContext options are configured
        /// in the Application/Server/Program.cs. These configuration
        /// options are passed to the DbContext base class using this
        /// constructor. 
        /// </param>
        /// <param name="operationalStoreOptions">Options for configuring
        /// the operational context. It enables persisting (and retrieving)
        /// operational data (Grants, Tokens, Device Flows).</param>
        /// <param name="httpContextAccessor">Provides access to the
        /// intrinsic HttpContext.Request, HttpContext.Response, and
        /// HttpContext.Server properties with current information
        /// about an individual Http request/response.</param>
        public AppDbContext(DbContextOptions<AppDbContext> options,
            IOptions<OperationalStoreOptions> operationalStoreOptions,
            IHttpContextAccessor httpContextAccessor)
            : base(options, operationalStoreOptions)
        {
            /// Retrieves the <em>ClaimsPrincipal</em> and user name
            /// <dfn>claim</dfn> (if any) from the current HttpContext
            /// to determine who performed thme operation (Http request).
            /// 
            /// HttpContext must be nullable to avoid any null reference
            /// exceptions during creation of the data model.
            _userName = httpContextAccessor.HttpContext?.User
                            .Identity?.Name
                        ?? "Unauthenticated User";
        }

        /// <summary>
        /// Represents in-memory <see cref="Genre"/> objects mapped to the
        /// corresponding database table. 
        /// </summary>
        public DbSet<Genre>? Genres { get; set; }

        /// <summary>
        /// Represents in-memory <see cref="Movie"/> objects mapped to the
        /// corresponding database table. 
        /// </summary>
        public DbSet<Movie>? Movies { get; set; }

        /// <summary>
        /// Represents in-memory <see cref="Person"/> objects mapped to the
        /// corresponding database table. 
        /// </summary>
        public DbSet<Person>? People { get; set; }

        /// <summary>
        /// Represents in-memory <see cref="MovieCharacter"/> objects mapped
        /// to the corresponding database table. 
        /// </summary>
        public DbSet<MovieCharacter>? MovieCharacters { get; set; }

        /// <summary>
        /// Represents in-memory <see cref="MovieScore"/> objects mapped to the
        /// corresponding database table. 
        /// </summary>
        public DbSet<MovieScore>? MovieScores { get; set; }

        /// <summary>
        /// Represents in-memory <see cref="PushSubscriptionDetails"/> objects
        /// mapped to the corresponding database table.
        /// </summary>
        public DbSet<PushSubscriptionDetails>?
            PushSubscriptionsDetails { get; set; }

        /// <summary>
        /// Overrides the the OnModelCreating method and uses the fluent API
        /// to configure the model without modifying the entity classes. 
        /// https://docs.microsoft.com/en-us/ef/core/modeling/
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            /// When overriding OnModelCreating, base.OnModelCreating should
            /// be called first. The overriding configuration should be called
            /// next. EF Core generally has a last-one-wins policy for
            /// configuration.
            /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/customize-identity-model?view=aspnetcore-6.0#customize-the-model
            base.OnModelCreating(modelBuilder);

            /// <remarks>
            /// If you were required to include a configuration setting
            /// for a particular entity from this method instead of
            /// defining it inside its configuration class, you can
            /// employ the modelBuilder.Entity<T>() method that
            /// returns an object that can be used to configure the
            /// entity passed to satisfy its type parameter.
            /// </remarks>
            PropertyInfo[] genresEntityProperties = modelBuilder
                .Entity<Genre>()
                .GetType()
                .GetProperties();

            /// Applies configurations from all your custom
            /// Microsoft.EntityFrameworkCore.IEntityTypeConfiguration/>
            /// instances that are defined in the EntityConfigurations
            /// folder of this assembly (Application/DataStore)
            modelBuilder
                .ApplyConfigurationsFromAssembly(
                    Assembly.GetExecutingAssembly());

            #region Shadow Properties

            /// Provides auditable and soft-deletable shadow properties
            /// to any root entity decorated with an IsAuditable attribute
            /// and an IsDeletable parameter value of "true".
            foreach (IMutableEntityType entityType in modelBuilder.Model
                             .GetEntityTypes())
            {
                /// Retrieves a custom attribute of the specified type
                /// that is applied to a specified member and optionally,
                /// inspects the ancestors of that member (bool value).
                /// Returns null if no such attribute is found.
                Attribute? attribute = entityType.ClrType
                        .GetCustomAttribute(typeof(IsAuditableAttribute), true);

                if (attribute is not IsAuditableAttribute customAttribute)
                    continue;

                /// Entity returns an object that can be used to configure
                /// a given entity in the model. 
                EntityTypeBuilder entity = modelBuilder
                    .Entity(entityType.Name);

                /// Designation of auditing shadow properties.
                entity.Property<DateTime>("CreatedOn");
                entity.Property<string>("CreatedBy");
                entity.Property<DateTime>("UpdatedOn");
                entity.Property<string>("UpdatedBy");

                /// Designation of soft-deleting shadow property based on the
                /// custom attribute's IsDeletable parameter value.
                if (customAttribute.IsDeletable)
                    entity.Property<bool>("IsDeleted");
            }

            /// <remarks>
            /// This approach attaches an "IsDeleted" shadow property
            /// to all entities in the data model. You chose to do it
            /// individually using each EntityConfiguration class for
            /// easier debugging and fine tunning further down the road.
            /// </remarks>
            /// 
            /// If no property exists with the same name in the
            /// entity class, a new shadow property will be added.
            ///foreach (IMutableEntityType entityType in 
            ///         modelBuilder.Model.GetEntityTypes())
            ///{
            ///    modelBuilder
            ///        .Entity(entityType.Name)
            ///        .Property<bool>("IsDeleted");

            ///    /// alternate approach
            ///    modelBuilder
            ///        .Entity(entityType.ClrType)
            ///        .Property<bool>("IsDeleted");
            ///}

            #endregion

        }

        /// <summary>
        /// Invokes the SaveChangesAsync method.
        /// </summary>
        /// <returns>Number of state entries written to the database.</returns>
        public override int SaveChanges()
        {
            InterceptAndSetEntityState();

            return base.SaveChanges();
        }

        /// <summary>
        /// Intercepts database transactions that intend to persist "Add",
        /// "Update", or "Delete" operations to set the <em>auditable</em> and
        /// <em>soft-deletable</em> shadow property values accordingly.
        /// </summary>
        /// <returns>Number of state entries written to the database.</returns>
        public override Task<int> SaveChangesAsync(
            bool acceptAllChangesOnSuccess,
            CancellationToken cancellationToken = default)
        {
            InterceptAndSetEntityState();

            return base.SaveChangesAsync(
                acceptAllChangesOnSuccess,
                cancellationToken);
        }

        /// <summary>
        /// Intercepts database transactions that intend to persist "Add",
        /// "Update" or "Delete" operations and sets the auditable and
        /// soft-deletable shadow property values accordingly. 
        /// </summary>
        private void InterceptAndSetEntityState()
        {
            /// Current date and time on the computer when a business
            /// transaction is completed and persisted into the database.
            DateTime timeStamp = DateTime.Now;

            /// Provides values for auditable and soft-deletable shadow
            /// properties to any root entity decorated with an IsAuditable
            /// attribute and an IsDeletable parameter value of "true".
            foreach (EntityEntry entry in ChangeTracker.Entries())
            {
                /// Retrieves a custom attribute of the specified type
                /// that is applied to a specified member and optionally,
                /// inspects the ancestors of that member (bool value).
                /// Returns null if no such attribute is found.
                Attribute? attribute = entry.Entity
                    .GetType()
                    .GetCustomAttribute(typeof(IsAuditableAttribute), true);

                if (attribute is not IsAuditableAttribute customAttribute)
                    continue;

                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property("CreatedBy").CurrentValue = _userName;
                        entry.Property("CreatedOn").CurrentValue = timeStamp;
                        entry.Property("UpdatedBy").CurrentValue = _userName;
                        entry.Property("UpdatedOn").CurrentValue = timeStamp;
                        if (customAttribute.IsDeletable)
                            entry.Property("IsDeleted").CurrentValue = false;
                        break;

                    case EntityState.Modified:
                        entry.Property("UpdatedBy").CurrentValue = _userName;
                        entry.Property("UpdatedOn").CurrentValue = timeStamp;
                        break;

                    case EntityState.Deleted:
                        if (customAttribute.IsDeletable)
                        {
                            entry.State = EntityState.Modified;
                            entry.Property("UpdatedBy").CurrentValue = _userName;
                            entry.Property("UpdatedOn").CurrentValue = timeStamp;
                            entry.Property("IsDeleted").CurrentValue = true;
                        }
                        break;
                }
            }
        }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.EnableDetailedErrors(true);
        //    optionsBuilder.EnableSensitiveDataLogging(true);
        //}
    }
}


