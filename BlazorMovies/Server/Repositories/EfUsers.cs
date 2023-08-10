using System.Security.Claims;
using System.Text;

using BlazorMovies.Client.ApiServices.IRepositories;
using BlazorMovies.Server.DataStore;
using BlazorMovies.Server.Helpers;
using BlazorMovies.Shared.AuthZHelpers;
using BlazorMovies.Shared.EDM;
using BlazorMovies.Shared.EntityDtos;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlazorMovies.Server.Repositories
{
    /// <summary>
    /// One application specific EfEntityName class for each 
    /// IEntityName interface exposed in the IUnitOfWork interface.
    /// </summary>
    /// <remarks>
    /// It is a subclass of the <see cref="EfRepository{TEntity}"/> class
    /// which means it inherits its general functionality applicable to all
    /// entities. 
    /// <para>
    /// This class is application specific and extends its base class
    /// with specific functionality for the type passed as type parameter.
    /// Anything related to 'eager loading' and 'explicit loading' belongs
    /// here; e.g., include related entities (and its property values) in 
    /// the result of a query with EF's "Include" extension method.
    /// </para>
    /// <para>
    /// Its "<c>internal</c>" access modifier makes it available only to
    /// elements that reside in the same assembly (project):
    /// Application/Server-Api
    /// </para>
    /// <para>
    /// Its methods have an "explicit interface implementation" to hide
    /// them from unwanted consumers. 
    /// </para>
    /// <para>
    /// It does not have an exception handling mechanism (try-catch blocks)
    /// because exceptions propagate up the stack until a catch statement for
    /// the exception is found. The Application/Server-Api/Controllers
    /// controller that calls method(s) in this repository has an exception
    /// handling mechanism. 
    /// </para>
    /// </remarks>
    internal class EfUsers : EfRepository<ApplicationUser>, IUsers
    {
        /// <summary>
        /// Property designed to have access to the unique DbContext inherited
        /// from its generic <see cref="EfRepository{TEntity}"/> parent class.
        /// This technique allows you to use the property for querying the DB
        /// instead of having to make an explicit conversion on each query (on
        /// each method). 
        /// </summary>
        /// <remarks>
        /// <see cref="Context"/> is a read-only protected field inherited from
        /// the parent class. This derived class employs its base class's
        /// DbContext instance as opposed to storing its constructor argument
        /// in a local variable to consume it. 
        /// <para>
        /// Every EfEntityName class that derives from the generic 
        /// <see cref="EfRepository{TEntity}"/> class employs its base class's
        /// DbContext protected field which is unique and represents a single
        /// session with the database that can have multiple operations with
        /// different entity types in a single business transaction. 
        /// </para>
        /// </remarks>
        private AppDbContext? AppContext => Context as AppDbContext;

        /// <summary>
        /// Provides the built-in APIs for managing User in a persistence
        /// store.
        /// </summary>
        private readonly UserManager<ApplicationUser> _userManager;

        /// <summary>
        /// Its formal input parameter <see cref="DbContext"/> is not stored in
        /// a local variable because it is not consumed like that. Instead, it
        /// is passed to satisfy its base class's constructor and it is the
        /// parent class which consumes it and also makes it available to any
        /// child class through a field named "Context" with a <c>"read-only
        /// protected"</c> access modifier.
        /// </summary>
        /// <param name="context">The application specific
        /// <see cref="DbContext"/> that represents a single and unique session
        /// with the database.
        /// </param>
        /// <param name="httpContextAccessor">Provides access to the current
        /// HttpContext.</param>
        /// <param name="userManager">Provides access to the built-in APIs for
        /// managing User in a persistence store.</param>
        public EfUsers(
            DbContext context,
            IHttpContextAccessor httpContextAccessor,
            UserManager<ApplicationUser> userManager)
        : base(context, httpContextAccessor)
        {
            _userManager = userManager;
        }

        #region Post-Create methods


        #endregion

        #region Get-Read methods

        /// <summary>
        /// Retrieves a sequence of user records in segments (or portions of
        /// data) that adhere to the specifications of the PaginationRequestDto. 
        /// </summary>
        /// <remarks>
        /// The sequence of type <see cref="UserDto"/> represents records of
        /// type <see cref="ApplicationUser"/>.
        /// </remarks>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page. </param>
        /// <returns>
        /// An object of type PaginatedResponseDto with database records after
        /// paginating the result of the database query. It includes the
        /// description and context of the paginated data (metadata). 
        /// </returns>
        async Task<PaginatedResponseDto<IEnumerable<UserDto>>?> IUsers.GetPaginatedUsersAsync(
                PaginationRequestDto paginationRequestDto)
        {
            if (paginationRequestDto is null)
                throw new ArgumentNullException(nameof(paginationRequestDto),
                    $"EfUsers {nameof(paginationRequestDto)} " +
                    $"cannot be null.");

            /// IQueryable<T> and IEnumerable<T> execute until they are
            /// consumed. However, IEnumerable cannot compose 'Expressions'
            /// and store them in an 'Expression Tree' to consume; i.e., with
            /// IEnumerable the system loads the results into memory for each
            /// query (or filter). 
            /// https://docs.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page?view=aspnetcore-6.0#iqueryable-vs-ienumerable
            IQueryable<ApplicationUser> usersQueryable = AppContext?.Users?
                .AsQueryable()!;

            /// The "Key" value for the custom HttpHeader to add to the
            /// HttpResponse with the pagination metadata. 
            const string metadataHttpHeaderTitle = "pagination-metadata";

            /// Custom extension method for IHttpContextAccessor interface
            /// formulates, serializes, and inserts the pagination metadata
            /// into the Http response in the form of a custom header.
            ///
            /// Metadata values must be captured before paginating the
            /// results. Otherwise the captured values (e.g.,
            /// totalExistingRecords) are based on the paginated results,
            /// not on the data source. 
            PaginationMetadata metadata = await HttpContextAccessor
                .InsertPaginationMetadataInResponse(
                    usersQueryable,
                    paginationRequestDto,
                    metadataHttpHeaderTitle);

            /// Custom extension method for IQueryable<T> interface determines
            /// which objects to include in the query result based on the
            /// expected number of records per page and the page number
            /// requesting the data.
            ///
            /// If sorting the collection in any shape or form is a must,
            /// regardless of the pagination method used, always make sure
            /// that the ordering is fully unique. If there can be multiple
            /// results with the same ordering value, then results could be
            /// skipped when paginating as they are ordered differently
            /// accross two paginating queries.
            /// https://docs.microsoft.com/en-us/ef/core/querying/pagination
            /// 
            /// This method executes sorting for illustrative purposes only.
            /// It is a primary ordering based on the object's unique primary
            /// Id to ensure consistent ordering on every query result. Subsequent
            /// sorting should be performed in the client layer.
            ///
            /// Note that the order of Linq methods matter. What's more, in
            /// this example, the whole query result is traversed to order the
            /// elements based on their primary key. If the collection has a
            /// lot of data, it can become a performance issue. You should consider
            /// using PLinq.
            /// https://stackoverflow.com/questions/7499384/does-the-order-of-linq-functions-matter
            usersQueryable = usersQueryable
                .OrderBy(u => u.Id)
                .Paginate(paginationRequestDto);

            /// Paginated response that includes the actual data and its metadata
            /// (description and context).  
            PaginatedResponseDto<IEnumerable<ApplicationUser>> paginatedResponseDto =
                new(usersQueryable, metadata);

            /// Maps ApplicationUser to UserDto items; i.e., it flattens
            /// the response data.
            IEnumerable<UserDto> paginatedUsers = paginatedResponseDto
                .ResponseData.Select(appUser =>
                    new UserDto()
                    {
                        Id = appUser.Id,
                        Email = appUser.Email
                    });

            /// Builds the PaginatedResponse with the metadata and the
            /// flattened data. 
            PaginatedResponseDto<IEnumerable<UserDto>>? paginatedUsersResponse =
                new(paginatedUsers, paginatedResponseDto.PaginationMetadata);

            return paginatedUsersResponse;
        }

        /// <summary>
        /// Retrieves a collection of all the custom <see cref="AuthZClaim"/>
        /// items available for controlling access to application resources and
        /// flags the authorization claims currently assigned to the user
        /// passed to satisfy the formal input parameter.
        /// </summary>
        /// <returns>
        /// A collection of all the custom <see cref="AuthZClaim"/>
        /// items available for controlling access to resources. It marks the
        /// authorization claims currently assigned to the user.</returns>
        async Task<UserClaimsDto?> IUsers.GetUserAuthZClaimsAsync(
            string userId)
        {
            /// Retrieves the User from the data store.
            ApplicationUser dbUser = await _userManager
                .FindByIdAsync(userId);

            if (dbUser == null)
            {
                throw new ArgumentNullException(
                    paramName: $"{userId}",
                    message: $"User with Id: {userId} not found");
            }

            IList<Claim> existingUserClaims = await _userManager
                .GetClaimsAsync(dbUser);

            /// Represents an AppliationUser.Id and a collection of
            /// AuthZClaimDto items.
            UserClaimsDto? userClaimsDto = new() { UserId = userId };

            /// Iterates over all the registered custom authorization claims.
            foreach (Claim claim in AuthZClaims.AllAuthZClaims)
            {
                /// Creates a type AuthZClaimDto with each custom authorization
                /// claim registered in the application.
                AuthZClaimDto authZClaimDto = new()
                {
                    Type = claim.Type,
                    Value = claim.Value
                };

                /// Flags the current AuthZClaimDto object if the user holds or
                /// has the custom authorization claim assigned.
                if (existingUserClaims.Any(c => c.Type == claim.Type))
                {
                    authZClaimDto.IsSelected = true;
                }

                /// The collection of all custom authorization claims registered
                /// in the application represented by AuthZClaimDto objects with
                /// a flag (bool property) if assigned to the user.
                userClaimsDto.AuthZClaimDtos.Add(authZClaimDto);
            }

            return userClaimsDto;
        }

        /// <summary>
        /// Queries the data store for a given <see cref="ApplicationUser"/>.
        /// </summary>
        /// <param name="userId">The <see cref="ApplicationUser"/>.Id of the
        /// user to retrieve from the data store.</param>
        /// <returns>A <see cref="UserDto"/> that represents an
        /// <see cref="ApplicationUser"/> Id and Email.</returns>
        async Task<UserDto?> IUsers.GetUserAsync(string userId)
        {
            /// Retrieves the User from the data store.
            ApplicationUser? dbUser = await _userManager
                .FindByIdAsync(userId);

            if (dbUser == null)
            {
                throw new ArgumentNullException(
                    paramName: $"{userId}",
                    message: $@"User with Id: {userId} not found");
            }

            UserDto? userDto = new()
            {
                Id = dbUser.Id,
                Email = dbUser.Email,
            };

            return userDto;
        }
        #endregion

        #region Put-Update methods

        /// <summary>
        /// Updates the custom authorization claims of a given User.
        /// </summary>
        /// <param name="userClaimsDto">A type that represents an
        /// <see cref="ApplicationUser"/>.Id and a collection of
        /// <see cref="AuthZClaimDto"/> items.</param>
        /// <returns>An ActionResult that represents the result of the action
        /// method.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        async Task<bool> IUsers.UpdateUserClaimsAsync(
            UserClaimsDto? userClaimsDto)
        {
            /// Retrieves the User from the data store.
            ApplicationUser dbUser = await _userManager
                .FindByIdAsync(userClaimsDto.UserId);

            if (dbUser == null)
            {
                throw new ArgumentNullException(
                    paramName: $"{userClaimsDto.UserId}",
                    message: $@"User with Id: {userClaimsDto.UserId} not found");
            }

            /// Gets a list of claims currently registered to the application
            /// user in the dbo.AspNetUserClaims table. It only retrieves custom
            /// authorization claims.
            IList<Claim> dbUserClaims = await _userManager
                .GetClaimsAsync(dbUser);

            /// Removes the complete set of custom authorization claims for the
            /// application user.
            IdentityResult result = await _userManager
                .RemoveClaimsAsync(dbUser, dbUserClaims);

            result = await _userManager
                .AddClaimsAsync(dbUser, userClaimsDto.AuthZClaimDtos
                    .Where(c => c.IsSelected)
                    .Select(c => new Claim(c.Type, c.Value)));

            return result.Succeeded;
        }

        #endregion

        #region Delete methods

        #endregion

    }
}


