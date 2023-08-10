
namespace BlazorMovies.Shared.QueryFilterDtos
{
    /// <summary>
    /// Defines the pagination specifications (which page and how many
    /// records) for an Http request for paginated data. 
    /// </summary>
    public class PaginationRequestDto
    {
        /// <summary>
        /// Default value for the number of records to serve in the Http
        /// response body. 
        /// </summary>
        private int _recordsPerPage = 10;

        /// <summary>
        /// Default value for the page number requested.
        /// </summary>
        private int _pageNumber = 1;
        
        /// <summary>
        /// Max number of records that can be served in the Http response
        /// body. 
        /// </summary>
        private const int _maxRecordsPerPage = 20;

        /// <summary>
        /// The page number that corresponds to the records that will be
        /// served. Its value cannot be zeroe or negative. 
        /// </summary>
        public int PageNumber
        {
            get => _pageNumber;

            set => _pageNumber = value < 1 
                ? 1 
                : value;
        }

        /// <summary>
        /// Limits the number of records served in the Http response body.
        /// </summary>
        public int RecordsPerPage
        {
            get => _recordsPerPage;

            set => _recordsPerPage = value > _maxRecordsPerPage 
                ? _maxRecordsPerPage 
                : value;
        }
    }
}

