
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

        // ================= REGISTER =================

        public async Task<IdentityResult> RegisterAsync(
            string email,
            string password)
        {
            email = email.Trim().ToLowerInvariant();

            var existingUser =
                await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "This email is already registered."
                    });
            }

            var user = new IdentityUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false
            };

            return await _userManager.CreateAsync(user, password);
        }

        // ================= LOGIN =================

        public async Task<bool> LoginAsync(
            string email,
            string password)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !user.EmailConfirmed)
                return false;

            var result = await _signInManager.PasswordSignInAsync(
                user,
                password,
                isPersistent: false,
                lockoutOnFailure: true
            );

            return result.Succeeded;
        }

        // ================= LOGOUT =================

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        // ================= EMAIL CONFIRMATION =================

        public async Task<string?> GenerateEmailConfirmationTokenAsync(
            string email)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.EmailConfirmed)
                return null;

            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        public async Task<IdentityResult> ConfirmEmailAsync(
            string userId,
            string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Invalid confirmation request."
                    });
            }

            return await _userManager.ConfirmEmailAsync(user, token);
        }

        // ================= GET USER ID =================

        public async Task<string?> GetUserIdByEmailAsync(string email)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);

            return user?.Id;
        }

        // ================= RESEND CONFIRMATION =================

        public async Task<string?> ResendEmailConfirmationAsync(
            string email)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || user.EmailConfirmed)
                return null;

            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        // ================= FORGOT PASSWORD =================

        public async Task<string?> GeneratePasswordResetTokenAsync(
            string email)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null || !user.EmailConfirmed)
                return null;

            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        // ================= RESET PASSWORD =================

        public async Task<IdentityResult> ResetPasswordAsync(
            string email,
            string token,
            string newPassword)
        {
            email = email.Trim().ToLowerInvariant();

            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                return IdentityResult.Failed(
                    new IdentityError
                    {
                        Description = "Invalid password reset request."
                    });
            }

            return await _userManager.ResetPasswordAsync(
                user,
                token,
                newPassword
            );
        }
    }
}
