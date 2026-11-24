using MailKit.Security;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using MailKit;

namespace WalletDiet.Services
{
    public class EmailService
    {
        private readonly EmailSettings _settings;

        public EmailService(IOptions<EmailSettings> options)
        {
            _settings = options.Value; 
        }

        public async Task SendEmailAsync(
            string to,
            string subject,
            string body)
        {
            // 1. Membuat objek email
            var email = new MimeMessage();

            // 2. Menentukan pengirim
            email.From.Add(
                new MailboxAddress("WalletDiet", _settings.SenderEmail)
            );

            // 3. Menentukan penerima
            email.To.Add(MailboxAddress.Parse(to));

            // 4. Menentukan subject
            email.Subject = subject;

            // 5. Menentukan isi email dalam format HTML
            email.Body = new TextPart("html")
            {
                Text = body
            };

            // 6. Membuat koneksi SMTP
            using var smtp = new MailKit.Net.Smtp.SmtpClient();

            await smtp.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                SecureSocketOptions.StartTls
            );

            // 7. Login ke Gmail
            await smtp.AuthenticateAsync(
                _settings.SenderEmail,
                _settings.AppPassword
            );

            // 8. Mengirim email
            await smtp.SendAsync(email);

            // 9. Memutus koneksi
            await smtp.DisconnectAsync(true);
        }

    }
}
