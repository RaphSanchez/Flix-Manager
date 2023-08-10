using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit.Text;
using MimeKit;

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Handles the code logic to send the Email using the Simple Mail Transfer
    /// Protocol (SMTP) server from ZeptoMail service and logs the response of
    /// the operation.
    /// </summary>
    /// <remarks>
    /// See YouTube video <see href="https://youtu.be/PvO_1T0FS_A">
    /// Send Email with a .Net 6 Web API using Mailkit & SMTP</see>,
    /// <see href="https://youtu.be/JzTxD4SczU8">
    /// How to send an email in C# with .Net using Mailkit</see>,
    /// and <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0">
    /// Implement IEmailSender</see>.
    /// </remarks>
    public class EmailSenderSmtp : IEmailSender
    {
        /// <summary>
        /// Represents a type used to perform logging.
        /// </summary>
        private readonly ILogger _logger;

        /// <summary>
        /// Custom class enables managing the configuration secrets (e.g.,
        /// user name and token) for the email service provider without
        /// exposing it in the source code.
        /// </summary>
        /// <remarks>
        /// The <see cref="AuthMessageSenderOptions"/> type was created to
        /// fetch the secure email key and token stored using the secret
        /// manager tool.
        /// </remarks>
        public AuthMessageSenderOptions Options { get; }

        /// <summary>
        /// Instantiates dependencies required to perform the email related
        /// operations.
        /// </summary>
        /// <param name="optionsAccessor">Enables retrieving instances of
        /// type <see cref="AuthMessageSenderOptions"/>.</param>
        /// <param name="logger">Enables logging operations.</param>
        public EmailSenderSmtp(
            IOptions<AuthMessageSenderOptions> optionsAccessor,
            ILogger<EmailSenderSmtp> logger)
        {
            Options = optionsAccessor.Value;
            _logger = logger;
        }

        /// <summary>
        /// Sends an email message using the SMTP protocol server from
        /// the ZeptoMail service.
        /// </summary>
        /// <param name="toEmail">Email address of the receiver.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="htmlMessage">Email body in HTML format.</param>
        /// <returns>An asynchronous operation.</returns>
        /// <exception cref="ArgumentNullException">Thrown if the ZeptoMail Key
        /// or ZeptoMail token are null.</exception>
        public async Task SendEmailAsync(
            string toEmail,
            string subject,
            string htmlMessage)
        {
            if (string.IsNullOrEmpty(Options.ZeptoMailKey)
                || string.IsNullOrEmpty(Options.ZeptoMailToken))
                throw new ArgumentNullException($"Null ZeptoMail Key or Token");

            /// Invokes helper method to execute send email operation.
            await Execute(
                Options.ZeptoMailKey,
                Options.ZeptoMailToken,
                toEmail,
                subject,
                htmlMessage);

        }

        /// <summary>
        /// Constructs the email message, authenticates and connects with the
        /// ZeptoMail SMTP server, and attempts to send the email.
        /// </summary>
        /// <param name="apiKey">Username for authentication with the SMTP
        /// server.</param>
        /// <param name="apiToken">Send mail token for authentication.</param>
        /// <param name="toEmail">Email address of the receiver.</param>
        /// <param name="subject">Email subject.</param>
        /// <param name="htmlBody">Email body in HTML format.</param>
        /// <returns>An asynchronous operation.</returns>
        public async Task Execute(
            string apiKey,
            string apiToken,
            string toEmail,
            string subject,
            string htmlBody)
        {
            /// 1. Construct email message.
            MimeMessage email = new();

            email.From.Add(new MailboxAddress(
                "FlixManager", "noreply@rafaelsanchez.ws"));

            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(TextFormat.Html)
            {
                Text = htmlBody
            };

            /// 2. Authenticate with server and send email.

            /// WARNING!: Ensure the use of MailKit.Net.Smtp.SmtpClient as
            /// opposed to System.Net.Mail.Smtp.
            SmtpClient smtpClient = new();

            try
            {
                /// Establishes a connection to the specified SMTP.
                await smtpClient.ConnectAsync(
                    host: "smtp.zeptomail.com",
                    port: 587,
                    SecureSocketOptions.StartTls);

                await smtpClient.AuthenticateAsync(
                    userName: apiKey,
                    password: apiToken);

                /// Sends the specified email and returns the final free-form
                /// text response from the server.
                string response = await smtpClient.SendAsync(email);

                _logger.LogInformation($"Email operation response: {response}");
            }
            catch (Exception ex)
            {
                /// Could log into a separate text file or somewhere else.
                Console.WriteLine(ex);

                _logger.LogError(
                    exception: ex, $"Sent email to ** {toEmail} ** failed.");
                throw;
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
                smtpClient.Dispose();
            }
        }
    }
}

