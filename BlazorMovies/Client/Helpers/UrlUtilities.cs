using System.Web;

namespace BlazorMovies.Client.Helpers
{
    /// <summary>
    /// Custom class provides methods to manipulate, encode, and decode URLs.
    /// </summary>
    public static class UrlUtilities
    {
        /// <summary>
        /// Constructs a URL encoded query string using a Dictionary with
        /// items that can be mapped to members of a DTO used as filtering
        /// criteria and pagination parameters. 
        /// </summary>
        /// <remarks>
        /// Only parameters applied as filtering criteria should contain
        /// actual values, the rest of the items in the dictionary collection
        /// should be initialized to their default values. Valid default
        /// values are <strong>"0" (zeroe), "string.Empty", and "false"
        /// </strong>.
        /// </remarks>
        /// <param name="queryStringParameters">A collection of
        /// &lt;Key, Value&gt; pairs used as parameters to build the query
        /// string. Valid values are <strong>"0" (zeroe), "string.Empty", and
        /// "false"</strong>.</param>
        /// <returns>A URL encoded query string. It includes the query string
        /// delimiter (?).</returns>
        public static string BuildEncodedQueryString(
            Dictionary<string, string?> queryStringParameters)
        {
            /// Holds the default values that are used as discriminators for
            /// the items in the Dictionary collection.  
            ///
            /// When a request is sent to the Application/Server-Api, only the
            /// parameters applied as filtering criteria are assigned actual
            /// values, the rest are initialized to their default values.
            ///
            /// The dictionary collection of <Key, Value> pairs with filtering
            /// parameters (queryStringParameters) is traversed against this
            /// default values collection to determine which parameters were
            /// assigned values; i.e., which parameters will be used to search
            /// the database and therefore should be included in the encoded
            /// query string. 
            List<string?> defaultValuesDiscriminator = new()
            { "0", string.Empty.ToLower(), "false" };

            /// Builds the query string segment for the web browser's URL as
            /// a collection of URL encoded Key=Value pairs where the Key is
            /// the name of the filtering parameter (e.g., Id, Title, Genre)
            /// and the Value is the actual value used to traverse the
            /// database records to try and find a match.
            /// https://chrissainty.com/working-with-query-strings-in-blazor/
            string encodedQueryString = string
                .Join("&", queryStringParameters
                    .Where(keyValuePair => !defaultValuesDiscriminator
                        .Contains(keyValuePair.Value?.ToLower()))
                    .Select(keyValuePair =>
                        $"{keyValuePair.Key.ToLower()}=" +
                        $"{HttpUtility.UrlEncode(keyValuePair.Value)?.ToLower()}"));

            return "?" + encodedQueryString;
        }

        /// <summary>
        /// Constructs a collection of type Dictionary with &lt;Key, Value&gt;
        /// pairs that can be mapped to members of a DTO used as filtering
        /// criteria and pagination parameters.  
        /// </summary>
        /// <param name="url">The URL to extract the values from.</param>
        /// <returns>A collection of &lt;Key, Value&gt; pairs that can be
        /// mapped to members of a DTO used as filtering criteria and
        /// pagination parameters. <strong>Its values are lower case</strong>.
        /// <para>
        /// Returns null if <paramref name="url"/> is null or does not contain
        /// a query string. 
        /// </para>
        /// </returns>
        public static Dictionary<string, string>? DecodeUrlQueryToDictionary(
            string url)
        {
            /// Ensures that the string has a query string format and that it
            /// has parameter values after the query delimiter.
            /// 
            /// (^) Index operator specifies the relative index from the end
            /// of an array.
            ///
            /// (..) Range operator specifies the start (inclusive) and end
            /// (exclusive) of a range.
            /// https://blog.ndepend.com/c-index-and-range-operators-explained/
            if (string.IsNullOrEmpty(url)
                || !url.Contains('?')
                || url[^1..] == "?")
            //|| url.Substring(url.Length - 1) == "?")
            {
                return null;
            }

            /// Splits into two sections the web browser's URL passed as an
            /// argument to this method. One with anything before and another
            /// one with anything after the query delimiter (?). Assumes URL
            /// has only one query delimiter.
            ///
            /// It only stores the second part of the URL (index [1]) which
            /// contains the query string parameters. (^) Index operator
            /// specifies the relative index from the end of an array.
            string queryParameters = url.Split("?")[^1];

            /// Collection of <Key,Value> pairs with one item for each query
            /// parameter extracted from the query string above. These
            /// parameters will be mapped to the members of a DTO that is used
            /// to represent the filtering criteria and the pagination
            /// parameters.
            ///
            /// The instructor employs a Uri.UnescapeDataString method instead
            /// of HttpUtility.UrlDecode. There is a long standing debate on
            /// which approach is better and why.
            Dictionary<string, string> parametersDict =
                queryParameters
                    .Split('&')
                    .ToDictionary(
                        // Key:
                        parameter =>
                            parameter.Split('=')[0],
                        // Value:
                        parameter =>
                            HttpUtility.UrlDecode(parameter.Split('=')[1]));

            return parametersDict;
        }
    }
}

