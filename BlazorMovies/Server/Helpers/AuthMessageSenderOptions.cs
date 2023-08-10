
using Microsoft.AspNetCore.Identity.UI.Services;

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Your custom class enables managing (sensitive information) to configure
    /// the implementations (<see cref="EmailSender"/>,
    /// <see cref="EmailSenderSmtp"/>) of the <see cref="IEmailSender"/>
    /// interface. 
    /// </summary>
    /// <remarks>
    /// Never store passwords or other sensitive data in source code.
    /// Production secrets should not be used for development or test.
    /// Secrets should not be deployed with the app. Instead, production
    /// secrets should be accessed through environment variables or
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/security/key-vault-configuration?view=aspnetcore-6.0">
    /// Azure Key Vault</see>.
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0">
    /// Configure an email provider</see>, 
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0">
    /// Safe storage of app secrets in development in ASP.Net Core</see>, and
    /// <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options?view=aspnetcore-6.0">
    /// Options Pattern</see>.
    /// </para>
    /// </remarks>
    public class AuthMessageSenderOptions
    {
        /// <summary>
        /// Represents the secure email key. It was set with the <see href="https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0">
        /// secret-manager tool</see> in the secret store. 
        /// </summary>
        public string? SendGridKey { get; set; }

        /// <summary>
        /// Represents the secure email key (user name) required by ZeptoMail
        /// email servers for authentication. It was set with the <see href="https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0">
        /// secret-manager tool</see> in the secret store. 
        /// </summary>
        public string? ZeptoMailKey { get; set; }

        /// <summary>
        /// Represents the secure email token required by ZeptoMail email
        /// servers for authentication. It was set with the <see href="https://docs.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-6.0">
        /// secret-manager tool</see> in the secret store. 
        /// </summary>
        public string? ZeptoMailToken { get; set; }
    }
}

