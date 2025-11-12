
namespace slick.Application.Services.Interfaces
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
        Task SendBulkSmsAsync(IEnumerable<string> phoneNumbers, string message);
        Task SendVerificationSmsAsync(string phoneNumber, string verificationCode);
        Task SendPasswordResetSmsAsync(string phoneNumber, string newPassword);
    }
}
