
using Microsoft.AspNetCore.Identity;

namespace CinemaBookingSystem.Services
{
    public interface IAccountService
    {
        Task<IdentityResult> RegisterAsync(
            string email,
            string password);

        Task<bool> LoginAsync(
            string email,
            string password);

        Task LogoutAsync();
    }
}
