// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace BlazorMovies.Server.Areas.Identity.Pages.Account
{
    /// <summary>
    ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    [AllowAnonymous]
    public class LockoutModel : PageModel
    {
        #region Lockout TimeSpan value

        /// Private backing field for the <see cref="LockOutTimeSpan"/>.
        private int _lockoutTimeSpan;

        /// <summary>
        /// Allocates the DefaultLockoutTimeSpan value into the
        /// <see cref="LockOutTimeSpan"/> member used by the Lockout page to
        /// inform the user the time to wait before trying to login again.
        /// </summary>
        /// <param name="idOptionsDelegate"><see cref="IOptions{TOptions}"/>
        /// retrieves configured <see cref="IdentityOptions"/> instances in
        /// the dependency injection container of the Application/Server-Api.
        /// See <see href="https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-6.0#configure-options-with-a-delegate">
        /// Configure options with a delegate</see>.
        /// </param>
        public LockoutModel(IOptions<IdentityOptions> idOptionsDelegate)
        {
            _lockoutTimeSpan = 
                idOptionsDelegate.Value.Lockout.DefaultLockoutTimeSpan.Minutes;
        }

        /// <summary>
        /// Provides a reference to the DefaultLockoutTimeSpan value configured
        /// in the dependency injection container of the Application/Server-Api.
        /// </summary>
        public int LockOutTimeSpan => _lockoutTimeSpan;

        #endregion

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public void OnGet()
        {
        }
    }
}

