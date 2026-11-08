
using Microsoft.AspNetCore.Identity;

namespace CinemaBookingSystem.Services
{
    public interface IAccountService
    {
        // ================= REGISTER =================

        Task<IdentityResult> RegisterAsync(
            string email,
            string password);

        // ================= LOGIN =================

        Task<bool> LoginAsync(
            string email,
            string password);

        // ================= LOGOUT =================

        Task LogoutAsync();

        // ================= EMAIL CONFIRMATION =================

        Task<string?> GenerateEmailConfirmationTokenAsync(
            string email);

        Task<IdentityResult> ConfirmEmailAsync(
            string userId,
            string token);

        Task<string?> GetUserIdByEmailAsync(
            string email);

        // ================= RESEND EMAIL CONFIRMATION =================

        Task<string?> ResendEmailConfirmationAsync(
            string email);

        // ================= FORGOT PASSWORD =================

        Task<string?> GeneratePasswordResetTokenAsync(
            string email);

        // ================= RESET PASSWORD =================

        Task<IdentityResult> ResetPasswordAsync(
            string email,
            string token,
            string newPassword);
    }
}
