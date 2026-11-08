
using CinemaBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace CinemaBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;
        private readonly IEmailSender _emailSender;

        public AccountController(
            IAccountService accountService,
            IEmailSender emailSender)
        {
            _accountService = accountService;
            _emailSender = emailSender;
        }

        // ================= REGISTER =================

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.RegisterAsync(
                model.Email,
                model.Password
            );

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                return View(model);
            }

            try
            {
                var token =
                    await _accountService.GenerateEmailConfirmationTokenAsync(
                        model.Email
                    );

                if (token == null)
                {
                    TempData["Error"] =
                        "Account created, but confirmation email could not be generated.";

                    return RedirectToAction(nameof(ResendEmailConfirmation));
                }

                var confirmationLink = Url.Action(
                    nameof(ConfirmEmail),
                    "Account",
                    new
                    {
                        email = model.Email,
                        token = token
                    },
                    Request.Scheme
                );

                var message = $@"
                    <h2>Welcome to Cinema Booking System!</h2>
                    <p>Thank you for creating an account.</p>
                    <p>Please confirm your email by clicking the link below:</p>
                    <a href='{confirmationLink}'>Confirm Email</a>
                    <p>If you did not create this account, ignore this email.</p>
                ";

                await _emailSender.SendEmailAsync(
                    model.Email,
                    "Confirm Your Email - Cinema Booking",
                    message
                );

                TempData["Success"] =
                    "Account created! Please check your email to confirm your account.";

                return RedirectToAction(nameof(Login));
            }
            catch
            {
                TempData["Error"] =
                    "Account created, but email delivery failed. Please try resending the confirmation email.";

                return RedirectToAction(nameof(ResendEmailConfirmation));
            }
        }

        // ================= CONFIRM EMAIL =================

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(
            string email,
            string token)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "Invalid confirmation link.";
                return RedirectToAction(nameof(Login));
            }

            // Retrieve the user ID through the service
            var userId = await GetUserIdAsync(email);

            if (userId == null)
            {
                TempData["Error"] = "Invalid confirmation request.";
                return RedirectToAction(nameof(Login));
            }

            var result = await _accountService.ConfirmEmailAsync(
                userId,
                token
            );

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Your email has been confirmed successfully! You can now log in.";
            }
            else
            {
                TempData["Error"] =
                    "Email confirmation failed. The link may be invalid or expired.";
            }

            return RedirectToAction(nameof(Login));
        }

        // ================= RESEND CONFIRMATION =================

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(
            ResendEmailConfirmationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var token =
                    await _accountService.ResendEmailConfirmationAsync(
                        model.Email
                    );

                if (token != null)
                {
                    var confirmationLink = Url.Action(
                        nameof(ConfirmEmail),
                        "Account",
                        new
                        {
                            email = model.Email,
                            token = token
                        },
                        Request.Scheme
                    );

                    var message = $@"
                        <h2>Email Confirmation</h2>
                        <p>Please confirm your email address:</p>
                        <a href='{confirmationLink}'>Confirm Email</a>
                    ";

                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "Resend Email Confirmation",
                        message
                    );
                }
            }
            catch
            {
                // Do not expose email delivery or account lookup details.
            }

            TempData["Success"] =
                "If an eligible account exists, a confirmation email has been sent.";

            return RedirectToAction(nameof(Login));
        }

        // ================= FORGOT PASSWORD =================

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(
            ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            try
            {
                var token =
                    await _accountService.GeneratePasswordResetTokenAsync(
                        model.Email
                    );

                if (token != null)
                {
                    var resetLink = Url.Action(
                        nameof(ResetPassword),
                        "Account",
                        new
                        {
                            email = model.Email,
                            token = token
                        },
                        Request.Scheme
                    );

                    var message = $@"
                        <h2>Password Reset</h2>
                        <p>We received a request to reset your password.</p>
                        <p>Click the link below to create a new password:</p>
                        <a href='{resetLink}'>Reset Password</a>
                        <p>If you did not request this, ignore this email.</p>
                    ";

                    await _emailSender.SendEmailAsync(
                        model.Email,
                        "Reset Your Password - Cinema Booking",
                        message
                    );
                }
            }
            catch
            {
                // Keep the response generic.
            }

            TempData["Success"] =
                "If an eligible account exists, a password reset email has been sent.";

            return RedirectToAction(nameof(Login));
        }

        // ================= RESET PASSWORD =================

        [HttpGet]
        public IActionResult ResetPassword(
            string email,
            string token)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(token))
            {
                TempData["Error"] = "Invalid password reset link.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            var model = new ResetPasswordViewModel
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
            ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _accountService.ResetPasswordAsync(
                model.Email,
                model.Token,
                model.Password
            );

            if (result.Succeeded)
            {
                TempData["Success"] =
                    "Password changed successfully! You can now log in.";

                return RedirectToAction(nameof(Login));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description
                );
            }

            return View(model);
        }

        // ================= LOGIN =================

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            return View(new LoginViewModel(default)
            {
                RedirectUrl = returnUrl
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Home");

            if (!ModelState.IsValid)
                return View(model);

            var success = await _accountService.LoginAsync(
                model.Email,
                model.Password
            );

            if (!success)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password, or your email has not been confirmed."
                );

                return View(model);
            }

            TempData["Success"] = "Welcome back!";

            if (!string.IsNullOrWhiteSpace(model.RedirectUrl) &&
                Url.IsLocalUrl(model.RedirectUrl))
            {
                return Redirect(model.RedirectUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        // ================= LOGOUT =================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();

            TempData["Success"] =
                "You have logged out successfully.";

            return RedirectToAction("Index", "Home");
        }

        // ================= ACCESS DENIED =================

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        // ================= HELPER =================

        private async Task<string?> GetUserIdAsync(string email)
        {
            // This method will be provided by AccountService.
            return await _accountService.GetUserIdByEmailAsync(email);
        }
    }

    // ================= VIEW MODELS =================

    public class RegisterViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password),
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class LoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? RedirectUrl { get; set; }
    }

    public class ResendEmailConfirmationViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password),
            ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
