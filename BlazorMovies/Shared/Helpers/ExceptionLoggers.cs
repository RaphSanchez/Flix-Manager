using System.Text;

namespace BlazorMovies.Shared.Helpers
{
    /// <summary>
    /// Encapsulates custom methods to log <see cref="Exception"/>s thrown.
    /// </summary>
    public class ExceptionLoggers
    {
        /// <summary>
        /// Represents a message for the application user when an unexpected
        /// error occurs.
        /// </summary>
        /// <remarks>
        /// Centralizing the message content facilitates future modifications.
        /// It also allows an easier implementation of localization
        /// (translation) services.
        /// </remarks>
        public const string MessageUnexpectedError =
            "An unexpected error occurred. Please try again.";

        /// <summary>
        /// Extracts the complete information of the exception passed as an
        /// argument including any inner exceptions. It employs a
        /// <see cref="StringBuilder"/> to construct the information and send
        /// it to the web browser's console for display.
        /// </summary>
        /// <param name="exception">The exception to work with.</param>
        public static void ExtractAndDisplayException(Exception exception)
        {
            /// These data is used for backend debugging and it could be
            /// logged into a database table or a .txt file for later
            /// review.
            ///
            /// You can refer to YouTube course "Ultimate ASP.Net
            /// Core Web API Tutorial for Beginners | Full Course" by Trevoir
            /// Williams for more info on logging exceptions. 
            /// https://youtube.com/playlist?list=PLUl9BcvgsrKYa9mUygO9lIGow-1GaNWqs
            StringBuilder sB = new($"Exception: {exception.GetType().Name}");
            sB.AppendLine();

            sB.Append($"Message: {exception.Message}");
            sB.AppendLine();

            sB.Append($"Stack Trace: {exception.StackTrace}");
            sB.AppendLine();

            Exception? innerException = exception.InnerException;
            while (innerException != null)
            {
                sB.Append($"Inner Ex: {innerException.GetType().Name}");
                sB.AppendLine();

                sB.Append($"Inner Message: {innerException.Message}");
                sB.AppendLine();

                sB.Append($"Inner StackTrace: {innerException.StackTrace}");
                sB.AppendLine();

                innerException = innerException.InnerException;
            }

            /// Full exception displayed into the web browser's console
            /// tools.
            string fullExceptionInfo = sB.ToString();
            Console.WriteLine(fullExceptionInfo);
        }
    }
}


