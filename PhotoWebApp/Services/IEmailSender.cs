namespace PhotoWebApp.Services
{
    public interface IEmailSender
    {
        // test email send interface
        Task SendEmailAsync(string email, string subject, string body);
    }
}
