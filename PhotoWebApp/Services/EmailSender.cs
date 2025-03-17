using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace PhotoWebApp.Services
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string body)
        {
            using var smtpClient = new SmtpClient();
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential("saitharun106@gmail.com", "guwepgyuhnfznpiy");
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;

            // message body
            var message = new MailMessage("saitharun106@gmail.com", email, subject, body);
            smtpClient.Send(message);

            return Task.CompletedTask;
        }
    }
}
