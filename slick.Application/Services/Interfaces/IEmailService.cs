using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace erp.Application.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendPasswordResetEmail(string email, string firstName, string newPassword);
        Task SendVerificationEmail(string email, string verificationToken, string userName );
        Task SendEmailNotificationAsync(IEnumerable<string> emails, string subject, string message, string userName = "User");
        Task SendEmailNotificationAsync(string email, string subject, string message, string userName = "User");
    }
}