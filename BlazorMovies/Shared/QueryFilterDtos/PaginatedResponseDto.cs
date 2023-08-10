using BlazorMovies.Shared.Helpers;

namespace BlazorMovies.Shared.QueryFilterDtos
{
    /// <summary>
    /// Encapsulates the object value served with an Http response that
    /// implements pagination. It includes metadata (description and context
    /// of the paginated data). 
    /// </summary>
    /// <typeparam name="T">The type of the object value; i.e., the type of
    /// the data served in the Http response body.</typeparam>
    public class PaginatedResponseDto<T>
    {
        /// <summary>
        /// Type has read-only properties (private setter methods); i.e., its
        /// formal input parameters can only be instantiated inside the
        /// containing class's constructor. 
        /// </summary>
        /// <param name="responseData">The actual data; i.e., the query result
        /// after being paginated. It is a read-only property.</param>
        /// <param name="paginationMetadata">Description and context of the
        /// paginated data.</param>
        public PaginatedResponseDto(
            T responseData, 
            PaginationMetadata paginationMetadata)
        {
            /// Parameter names in the constructor must match with a
            /// property or field on the object (PaginationMetadata).
            /// This means that each name of the formal input parameters
            /// in the constructor must match a property or field name.
            /// The match can be case-insensitive.
            /// 
            /// Otherwise, System.Text.Json serializer won't be able to
            /// bind to an object property or field on deserialization.
            ResponseData = responseData;

            PaginationMetadata = paginationMetadata;
        }

        /// <summary>
        /// The actual data; i.e., the query result after being paginated. It
        /// is a read-only property (private setter method), it can only be
        /// instantiated inside the containing class.
        /// </summary>
        public T ResponseData { get; private set; }

        /// <summary>
        /// Pagination metadata. It is a read-only property (private setter
        /// method), it can only be instantiated inside the containing class.
        /// </summary>
        public PaginationMetadata PaginationMetadata { get; private set; }
    }
}