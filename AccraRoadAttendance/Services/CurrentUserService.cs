using Microsoft.AspNetCore.Identity;
using AccraRoadAttendance.Models;

namespace AccraRoadAttendance.Services
{
    public class CurrentUserService
    {
        private readonly UserManager<User> _userManager;
        public User CurrentUser { get; private set; }

        public CurrentUserService(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null && await _userManager.CheckPasswordAsync(user, password))
            {
                CurrentUser = user;
                return true;
            }
            return false;
        }

        public void Logout()
        {
            CurrentUser = null;
        }

        public bool IsLoggedIn => CurrentUser != null;

        public async Task<bool> IsInRoleAsync(string role)
        {
            return CurrentUser != null && await _userManager.IsInRoleAsync(CurrentUser, role);
        }
    }
}