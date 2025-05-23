using Domain.Interfaces;
using Infrastructure.HelperServices.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using System.Net.Mail;

namespace Infrastructure.HelperServices
{
    public class EmailService : IEmailRepository
    {
        private readonly EmailServiceDTO _settings;

        public EmailService(IOptions<EmailServiceDTO> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendInvoiceEmailAsync(string toEmail, string subject, string htmlContent)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("Shop Invoice", _settings.FromEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlContent
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_settings.FromEmail, _settings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
