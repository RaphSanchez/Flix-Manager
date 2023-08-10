namespace BlazorMovies.Client.Shared
{
    public partial class NavMenu
    {
        /// <summary>
        /// Pre-built code.
        /// </summary>
        private bool collapseNavMenu = true;
        private string? NavMenuCssClass => collapseNavMenu ? "collapse" : null;
        private void ToggleNavMenu()
        {
            collapseNavMenu = !collapseNavMenu;
        }

        /// <summary>
        /// Custom code to show/hide the NavFlix nav menu item and its sub-menu
        /// items.
        /// </summary>
        private bool _showFlixMenu = false;
        private bool _showComponentsMenu = false;
        private bool _showFormsMenu = false;
        private bool _showApiMenu = false;
        private bool _showConsumingApiMenu = false;
        private bool _showSecurityMenu = false;
        private bool _showUserManagementMenu = false;
        private bool _showMiscellaneousMenu = false;
        private bool _showDeploymentMenu = false;
        private bool _showRobustifyingApiMenu = false;
        private bool _showPwaMenu = false;
        private bool _showPushNotificationsMenu = false;

        private void HideSubmenus()
        {
            _showFlixMenu = false;
            _showComponentsMenu = false;
            _showFormsMenu = false;
            _showApiMenu = false;
            _showConsumingApiMenu = false;
            _showSecurityMenu = false;
            _showUserManagementMenu = false;
            _showMiscellaneousMenu = false;
            _showDeploymentMenu = false;
            _showRobustifyingApiMenu = false;
            _showPwaMenu = false;
            _showPushNotificationsMenu = false;
        }

        private void ToggleFlixSubmenu()
        {
            HideSubmenus();
            _showFlixMenu = !_showFlixMenu;
        }

        private void ToggleComponentsSubmenu()
        {
            HideSubmenus();
            _showComponentsMenu = !_showComponentsMenu;
        }

        private void ToggleFormsSubmenu()
        {
            HideSubmenus();
            _showFormsMenu = !_showFormsMenu;
        }

        private void ToggleApiSubmenu()
        {
            HideSubmenus();
            _showApiMenu = !_showApiMenu;
        }

        private void ToggleConsumingApiSubmenu()
        {
            HideSubmenus();
            _showConsumingApiMenu = !_showConsumingApiMenu;
        }

        private void ToggleSecuritySubmenu()
        {
            HideSubmenus();
            _showSecurityMenu = !_showSecurityMenu;
        }

        private void ToggleIdentityUserSubmenu()
        {
            HideSubmenus();
            _showUserManagementMenu = !_showUserManagementMenu;
        }

        private void ToggleMiscellaneousSubmenu()
        {
            HideSubmenus();
            _showMiscellaneousMenu = !_showMiscellaneousMenu;
        }

        private void ToggleDeploymentSubmenu()
        {
            HideSubmenus();
            _showDeploymentMenu = !_showDeploymentMenu;
        }

        private void ToggleRobustifyingApiSubmenu()
        {
            HideSubmenus();
            _showRobustifyingApiMenu = !_showRobustifyingApiMenu;
        }

        private void TogglePwaSubmenu()
        {
            HideSubmenus();
            _showPwaMenu = !_showPwaMenu;
        }

        private void TogglePushNotificationsSubmenu()
        {
            HideSubmenus();
            _showPushNotificationsMenu = !_showPushNotificationsMenu;
        }
    }
}
