using AccraRoadAttendance.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace AccraRoadAttendance.Services
{
    public class CurrentUserService
    {
        //private readonly UserManager<User> _userManager;
        private readonly IServiceProvider _serviceProvider;
        private ClaimsPrincipal _currentPrincipal;
        private readonly object _lock = new object();
        //public ClaimsPrincipal CurrentPrincipal { get; private set; }

        public CurrentUserService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ClaimsPrincipal CurrentPrincipal
        {
            get
            {
                lock (_lock)
                {
                    return _currentPrincipal;
                }
            }
            private set
            {
                lock (_lock)
                {
                    _currentPrincipal = value;
                }
            }
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            // Create a scope to resolve scoped services
            using var scope = _serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();

            var user = await userManager.FindByEmailAsync(email);
            if (user != null && await userManager.CheckPasswordAsync(user, password))
            {
                // Create claims for the user
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.UserName)
                };

                // Add roles as claims
                var roles = await userManager.GetRolesAsync(user);
                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                // Create identity and principal
                var identity = new ClaimsIdentity(claims, "Password");
                CurrentPrincipal = new ClaimsPrincipal(identity);

                return true;
            }
            return false;
        }

        public void Logout()
        {
            CurrentPrincipal = null;
        }

        public bool IsLoggedIn => CurrentPrincipal != null;

        public bool IsInRole(string role)
        {
            return CurrentPrincipal?.IsInRole(role) ?? false;
        }

        public bool HasClaim(string type, string value)
        {
            return CurrentPrincipal?.HasClaim(type, value) ?? false;
        }

        public string GetUserId()
        {
            return CurrentPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string GetUserEmail()
        {
            return CurrentPrincipal?.FindFirst(ClaimTypes.Email)?.Value;
        }
    }
}