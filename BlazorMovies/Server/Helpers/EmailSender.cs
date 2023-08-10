using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;

using SendGrid;
using SendGrid.Helpers.Mail;

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Handles the code logic to send the Email and logs the StatusCode of
    /// the <see cref="Response"/>.
    /// </summary>
    /// <remarks>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0">
    /// Implement IEmailSender</see>.
    /// </remarks>
    public class EmailSender : IEmailSender
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Custom class enables managing the <see cref="SendGridKey"/>
        /// (sensitive information) in a development machine.
        /// </summary>
        /// <remarks>
        /// The <see cref="AuthMessageSenderOptions"/> type was created to
        /// fetch the secure email key stored using the secret manager tool.
        /// </remarks>
        public AuthMessageSenderOptions Options { get; }

        public EmailSender(
            IOptions<AuthMessageSenderOptions> optionsAccessor,
            ILogger<EmailSender> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlMessage)
        {
            if (string.IsNullOrEmpty(Options.SendGridKey))
                throw new Exception("Null SendGridKey");
            await Execute(Options.SendGridKey, subject, htmlMessage, toEmail);
        }

        public async Task Execute(
            string apiKey,
            string subject,
            string htmlMessage,
            string toEmail)
        {
            /// An HttpClient wrapper for interacting with Twilio SendGrid's
            /// API.
            SendGridClient client = new SendGridClient(apiKey);

            /// Builds an object that sends an email through Twilio SendGrid.
            SendGridMessage message = new SendGridMessage()
            {
                From = new EmailAddress("contacto@rafaelsanchez.ws", "Flix Manager"),
                Subject = subject,
                PlainTextContent = htmlMessage,
                HtmlContent = htmlMessage,
            };

            /// Adds a recipient email.
            message.AddTo(new EmailAddress(toEmail));

            // Disable-Enable click tracking.
            // See https://sendgrid.com/docs/User_Guide/Settings/tracking.html
            message.SetClickTracking(true, false);

            /// The response received from an API call to Twilio SendGrid.
            Response response = await client.SendEmailAsync(message);

            _logger.LogInformation(response.IsSuccessStatusCode
            ? $"Email to {toEmail} queued successfully."
            : $"Failure Email to {toEmail}");
        }
    }
}

