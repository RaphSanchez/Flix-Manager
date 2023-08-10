using System;
using System.Linq;

namespace BlazorMovies.Client.Helpers
{
    public static class StringExtensions
    {
        /// <summary>
        /// Custom extension method for types <strong>string</strong>.
        /// It Validates if a file name has an image extension of type
        /// .jpeg, .jpg, or .png.
        /// </summary>
        /// <param name="fileContentType">A file's MIME type specified by
        /// the browser.</param>
        /// <returns>True if the extension is of type image. False if it's
        /// not.</returns>
        public static bool HasValidImageExtension(this string fileContentType)
        {
            string[] splitContentType = fileContentType.Split('.');

            string[] validExtensions = { "jpeg", "jpg", "png" };

            /// Uses an "Index from end" expression.
            return validExtensions.Contains(splitContentType[^1]);
            //return validExtensions.Contains(splitContentType[splitContentType.Length - 1]);
        }

        /// <summary>
        /// Custom extension method tests if the string is Base64 encoded.
        /// </summary>
        /// <remarks>
        /// <para>
        /// For more info visit: <see href="https://stackoverflow.com/questions/6309379/how-to-check-for-a-valid-base64-encoded-string">
        /// How to check for a valid Base64 string.</see>
        /// </para>
        /// </remarks>
        /// <param name="base64String"></param>
        /// <returns>True if the string is Base64 encoded, otherwise false.
        /// </returns>
        public static bool IsBase64(this string base64String)
        {
            if (!string.IsNullOrEmpty(base64String))
            {
                try
                {
                    Convert.FromBase64String(base64String);
                    return true;
                }
                catch (Exception e)
                {
                    string message = e.Message;
                    return false;
                }
            }
            return false;
        }
    }
}


