using System.Threading.Tasks;
using PhotoWebApp.Services;

namespace PhotoWebApp.IntegrationTests.Mocks
{
    public class MockEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string body)
        {
            // Log or mock email sending for testing purposes
            Console.WriteLine($"Mock email sent to {email} with subject {subject} and body {body}");
            return Task.CompletedTask;
        }
    }
}
