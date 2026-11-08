using System.Net;
using System.Net.Mail;
using System.Text;

namespace CinemaBookingSystem.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendEmailAsync(
            string email,
            string subject,
            string message)
        {
            var host = _configuration["EmailSettings:Host"];
            var port = _configuration.GetValue<int>("EmailSettings:Port");
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var fromEmail = _configuration["EmailSettings:FromEmail"];
            var fromName = _configuration["EmailSettings:FromName"];

            if (string.IsNullOrWhiteSpace(host) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail) ||
                port <= 0)
            {
                throw new InvalidOperationException(
                    "Email SMTP settings are missing or invalid."
                );
            }

            using var mail = new MailMessage();

            mail.From = new MailAddress(
                fromEmail,
                fromName ?? "CinemaBookingSystem"
            );

            mail.To.Add(email);
            mail.Subject = subject;
            mail.Body = message;
            mail.IsBodyHtml = true;
            mail.BodyEncoding = Encoding.UTF8;
            mail.SubjectEncoding = Encoding.UTF8;

            using var smtp = new SmtpClient(host, port);

            smtp.Credentials = new NetworkCredential(
                username,
                password
            );

            smtp.EnableSsl =
                _configuration.GetValue<bool>("EmailSettings:EnableSsl");

            await smtp.SendMailAsync(mail);
        }
    }
}