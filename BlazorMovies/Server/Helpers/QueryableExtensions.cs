using BlazorMovies.Shared.QueryFilterDtos;

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Custom class extends the functionality of the built-in
    /// IQueryable<typeparam name="T">&lt;T&gt;</typeparam> interface
    /// which is used to evaluate queries against a specific data
    /// source. 
    /// </summary>
    internal static class QueryableExtensions
    {
        /// <summary>
        /// Determines which objects to include in the query result based
        /// on the expected number of records per page and the page number
        /// requesting the data.
        /// </summary>
        /// <typeparam name="T">The type of the object value; the type of
        /// the data served in the Http response body.</typeparam>
        /// <param name="queryable">The query result with the complete
        /// set of available items in the database.</param>
        /// <param name="paginationRequestDto">Pagination parameters; e.g.,
        /// page number and number of records per page.</param>
        /// <returns>The portion or segment of object items to include in
        /// the query results after the pagination parameters have been
        /// applied.
        /// </returns>
        /// <exception cref="ArgumentNullException">Exception thrown if the
        /// argument to satisfy its formal input parameter is null.
        /// </exception>
        public static IQueryable<T> Paginate<T>(this IQueryable<T> queryable,
            PaginationRequestDto paginationRequestDto)
        {
            if (paginationRequestDto == null)
                throw new ArgumentNullException(nameof(paginationRequestDto),
                    $"Instance of {nameof(paginationRequestDto)} cannot be null.");

            return queryable
                .Skip((paginationRequestDto.PageNumber - 1) *
                      paginationRequestDto.RecordsPerPage)
                .Take(paginationRequestDto.RecordsPerPage);
        }
    }
}

/// https://stackoverflow.com/questions/47098782/how-to-get-primary-keys
//string primaryKeyName = string.Empty;
//PropertyInfo[] properties = typeof(T).GetType().GetProperties();

//foreach (PropertyInfo property in properties)
//{

//    IEnumerable<Attribute> keyAttribute = property
//        .GetCustomAttributes(typeof(KeyAttribute));
//    if (!keyAttribute.Any()) continue;

//    primaryKeyName = property.Name;
//    break;
//}


