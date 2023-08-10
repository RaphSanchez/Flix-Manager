using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace BlazorMovies.Server.Helpers
{
    /// <summary>
    /// Used to change the email token lifespan. It derives from
    /// <see cref="DataProtectorTokenProvider{TUser}"/> type that provides
    /// protection and validation of identity tokens; i.e., it inherits
    /// all the functionality of its parent class and uses it to pass the
    /// options defined by the custom
    /// <see cref="EmailConfirmationTokenProviderOptions"/>. 
    /// </summary>
    /// <remarks>
    /// <see cref="EmailConfirmationTokenProviderOptions"/> set the email token
    /// lifespan to 3 days.
    /// <para>
    /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/security/authentication/accconfirm?view=aspnetcore-6.0">
    /// Change the email token lifespan</see>.
    /// </para>
    /// </remarks>
    /// <typeparam name="TUser">The type used to represent a User.</typeparam>
    public class CustomEmailConfirmationTokenProvider<TUser>
        : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CustomEmailConfirmationTokenProvider(
            IDataProtectionProvider dataProtectionProvider,
            IOptions<EmailConfirmationTokenProviderOptions> options,
            ILogger<DataProtectorTokenProvider<TUser>> logger)
        : base(dataProtectionProvider, options, logger)
        { }
    }

    /// <summary>
    /// Defines the options for the
    /// <see cref="CustomEmailConfirmationTokenProvider{TUser}"/> that sets
    /// the email token lifespan to 3 days. 
    /// </summary>
    /// <remarks>
    /// The name allocated for the DataProtectorTokenProvider is:
    /// "EmailDataProtectorTokenProvider".
    /// </remarks>
    public class EmailConfirmationTokenProviderOptions :
        DataProtectionTokenProviderOptions
    {
        public EmailConfirmationTokenProviderOptions()
        {
            /// Key:Value pair sets the name of the
            /// DataProtectorTokenProvider<TUser>. This Key is used to
            /// call the custom user token provider that should be
            /// constructed to pass the options defined here. 
            Name = "EmailDataProtectorTokenProvider";

            /// Amount of time a generated token remains valid. It defaults
            /// to 1 day. 
            TokenLifespan = TimeSpan.FromDays(3);
        }
    }
}

