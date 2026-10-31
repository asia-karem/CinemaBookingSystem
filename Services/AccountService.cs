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
            email = email.Trim().ToLowerInvariant();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "This email is already registered."
                    }
                );
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email
            };

            // New registrations are regular users, not Admins.
            return await _userManager.CreateAsync(user, password);
        }

        public async Task<bool> LoginAsync(
            string email,
            string password)
        {
            email = email.Trim();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return false;

            var result = await _signInManager.PasswordSignInAsync(
                user,
                password,
                isPersistent: false,
                lockoutOnFailure: true
            );

            return result.Succeeded;
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        public Task RegisterAsync(string email, object password)
        {
            throw new NotImplementedException();
        }
    }
}