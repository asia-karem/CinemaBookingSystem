
using Microsoft.AspNetCore.Identity;

namespace CinemaBookingSystem.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;

        public AccountService(
            UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<IdentityResult> RegisterAsync(
            string email,
            string password)
        {
            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            return await _userManager.CreateAsync(user, password);
        }

        public async Task<bool> LoginAsync(
            string email,
            string password)
        {
            var result = await _signInManager.PasswordSignInAsync(
                email,
                password,
                isPersistent: false,
                lockoutOnFailure: true);

            return result.Succeeded;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}
