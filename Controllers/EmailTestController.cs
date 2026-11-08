
using CinemaBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace CinemaBookingSystem.Controllers
{
    public class EmailTestController : Controller
    {
        private readonly IEmailSender _emailSender;

        public EmailTestController(IEmailSender emailSender)
        {
            _emailSender = emailSender;
        }

        public async Task<IActionResult> Send()
        {
            try
            {
                await _emailSender.SendEmailAsync(
                    "karemasia45@gmail.com",
                    "SMTP Test - Cinema Booking System",
                    "<h2>SMTP is working!</h2><p>Your email service is ready.</p>"
                );

                return Content("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return Content("Email sending failed: " + ex.Message);
            }
        }
    }
}
