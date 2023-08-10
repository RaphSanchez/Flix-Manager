using System.Text.Json;
using BlazorMovies.Shared.Helpers;
using BlazorMovies.Shared.QueryFilterDtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Primitives;

#pragma warning disable CS1587

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Custom class extends the functionality of the built-in
    /// IHttpContextAccessor interface which provides access to the intrinsic
    /// HttpContext.Request, HttpContext.Response, and HttpContext.Server
    /// properties with info about the current Http request/response/server.
    /// </summary>
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Formulates, serializes, and inserts the pagination metadata
        /// (description and context of the paginated data) into the Http
        /// response in the form of a custom header.
        /// </summary>
        /// <typeparam name="T">The type of the data source.</typeparam>
        /// <param name="httpContextAccessor">Instance of an
        /// IHttpContextAccessor that provides access to the intrinsic
        /// HttpContext.Request, HttpContext.Response, and
        /// HttpContext.Server properties with info about the current Http
        /// request/response.</param>
        /// <param name="queryable">The result of a database query with the
        /// object values that will be paginated.</param>
        /// <param name="paginationRequestDto">Defines pagination
        /// parameters such as number of page and number of records per
        /// page.</param>
        /// <param name="metadataHttpHeaderTitle">The "Key" value for the
        /// custom HttpHeader to add to the HttpResponse with the pagination
        /// metadata.</param>
        /// <returns>A PaginationMetadata object with the description and
        /// context of the paginated data.</returns>
        /// <exception cref="ArgumentNullException">Exception thrown if the
        /// argument to satisfy its formal input parameter is null.
        /// </exception>
        internal static async Task<PaginationMetadata> 
            InsertPaginationMetadataInResponse<T>(
                this IHttpContextAccessor httpContextAccessor,
                IQueryable<T>? queryable,
                PaginationRequestDto paginationRequestDto,
                string metadataHttpHeaderTitle)
        {
            if (httpContextAccessor == null)
                throw new ArgumentNullException(nameof(httpContextAccessor),
                    $"Instance of {nameof(httpContextAccessor)} cannot be null.");

            if (queryable == null)
                throw new ArgumentNullException(nameof(queryable),
                    $"Instance of {nameof(queryable)} cannot be null");

            /// The total number of records currently available in the database. 
            double totalItemsCount = await queryable.CountAsync();

            /// Calculates the total number of pages (or segments) required to
            /// evenly distribute the total number of records available in the
            /// database attending the specifications outlined in the
            /// PaginationRequestDto (e.g., records per page).
            int totalPages = (int)Math
                .Ceiling(totalItemsCount / paginationRequestDto.RecordsPerPage);

            /// Description and context of the paginated data.
            PaginationMetadata metadata = new(
                paginationRequestDto.RecordsPerPage,
                paginationRequestDto.PageNumber,
                (int)totalItemsCount,
                totalPages);

            /// Inserts the pagination metadata into the Http response in the
            /// form of a custom Http header. It employs a JsonSerializer to
            /// serialize the metadata values. 
            /// https://code-maze.com/aspnetcore-add-custom-headers/
            /// https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-how-to?pivots=dotnet-6-0
            ///
            /// If you get an exception related with an invalid ASCII
            /// character, try removing the hyphen (-) from the
            /// metadataHttpHeaderTitle.
            httpContextAccessor.HttpContext.Response.Headers
                .Add(metadataHttpHeaderTitle, JsonSerializer
                    .Serialize<PaginationMetadata>(metadata));

            return metadata;
        }
    }
}



