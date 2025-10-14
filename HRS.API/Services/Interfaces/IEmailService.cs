namespace HRS.API.Services.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string recipient, string subject, string body, bool isHtml = true);
    Task SendVerificationEmailAsync(string email, string verificationToken, string firstName);
    Task SendPasswordResetEmailAsync(string email, string resetToken, string firstName);
    Task SendEmployeeWelcomeEmailAsync(string email, string password, string firstName);
}
