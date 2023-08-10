using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazorMovies.Server.Controllers
{
    /// <summary>
    /// Retrieves the Application/Client parameters for the Client instance
    /// making the Http request; i.e., the Client endpoint is provisioned to
    /// serve OIDC parameters.
    /// </summary>
    /// <remarks>
    /// Parameters include "Authority" (or domain), "Client.Id", 
    /// "RedirectUri", "PostLogoutRedirectUri", "ResponseType", and
    /// "AllowedScopes".  
    /// </remarks>
    public class OidcConfigurationController : Controller
    {
        /// https://docs.microsoft.com/en-us/aspnet/core/blazor/security/webassembly/hosted-with-identity-server?view=aspnetcore-6.0&tabs=visual-studio#oidcconfigurationcontroller
        /// https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-6.0#application-profiles
        
        private readonly ILogger<OidcConfigurationController> _logger;

        public OidcConfigurationController(
            IClientRequestParametersProvider clientRequestParametersProvider,
            ILogger<OidcConfigurationController> logger)
        {
            ClientRequestParametersProvider = clientRequestParametersProvider;
            _logger = logger;
        }

        public IClientRequestParametersProvider
            ClientRequestParametersProvider { get; }

        [HttpGet("_configuration/{clientId}")]
        /// 
        public IActionResult GetClientRequestParameters(
            [FromRoute] string clientId)
        {
            /// Set of conventional values to configure the IdentityServer
            /// engine.
            IDictionary<string, string> parameters =
                ClientRequestParametersProvider
                    .GetClientParameters(HttpContext, clientId);

            return Ok(parameters);
        }
    }
}



