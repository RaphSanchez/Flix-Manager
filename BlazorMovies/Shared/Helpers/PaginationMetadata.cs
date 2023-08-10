
namespace BlazorMovies.Shared.Helpers
{
    /// <summary>
    /// Description and context of the paginated data.
    /// </summary>
    public class PaginationMetadata
    {
        public PaginationMetadata(
            int recordsPerPage, 
            int pageNumber, 
            int totalExistingRecords, 
            int totalPages)
        {
            /// Parameter names in the constructor must match with a
            /// property or field on the object (PaginationMetadata).
            /// This means that each name of the formal input parameters
            /// in the constructor must match a property or field name.
            /// The match can be case-insensitive.
            /// 
            /// Otherwise, System.Text.Json serializer won't be able to
            /// bind to an object property or field on deserialization.
            RecordsPerPage = recordsPerPage;
            PageNumber = pageNumber;
            TotalExistingRecords = totalExistingRecords;
            TotalPages = totalPages;
        }

        // Read-only properties (private setter method). They can only be
        // instantiated inside the containing class. 

        public int RecordsPerPage { get; private set; }
        public int PageNumber { get; private set; }
        public int TotalExistingRecords { get; private set; }
        public int TotalPages { get; private set; }
        public bool HasPrevious => PageNumber > 1;
        public bool HasNext => PageNumber < TotalPages;
    }
}