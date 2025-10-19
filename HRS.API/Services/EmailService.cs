using HRS.API.Services.Interfaces;

namespace HRS.API.Services;

public class EmailService : IEmailService
{
    private readonly IEmailSenderService _emailSender;
    private readonly IEmailBuilderService _emailBuilder;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        IEmailSenderService emailSender,
        IEmailBuilderService emailBuilder,
        ILogger<EmailService> logger)
    {
        _emailSender = emailSender;
        _emailBuilder = emailBuilder;
        _logger = logger;
    }

    public async Task SendEmailAsync(string recipient, string subject, string body, bool isHtml = true)
    {
        if (string.IsNullOrWhiteSpace(recipient))
            throw new ArgumentException("Recipient is required", nameof(recipient));

        var success = await _emailSender.SendEmailAsync(recipient, subject, body, isHtml);
        
        if (!success)
        {
            _logger.LogError("Failed to send simple email to {Recipient}", recipient);
            throw new InvalidOperationException($"Failed to send email to {recipient}");
        }

        _logger.LogInformation("Successfully sent email to {Recipient}", recipient);
    }

    public async Task SendVerificationEmailAsync(string email, string verificationToken, string firstName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(verificationToken))
            throw new ArgumentException("Verification token is required", nameof(verificationToken));

        var template = _emailBuilder.BuildVerificationEmailTemplate(email, verificationToken, firstName);
        var body = _emailBuilder.GenerateEmailBody(template);
        
        var success = await _emailSender.SendEmailAsync(email, template.Title, body, isHtml: true);
        
        if (!success)
        {
            _logger.LogError("Failed to send verification email to {Email}", email);
            throw new InvalidOperationException($"Failed to send verification email to {email}");
        }

        _logger.LogInformation("Successfully sent verification email to {Email}", email);
    }

    public async Task SendPasswordResetEmailAsync(string email, string resetToken, string firstName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(resetToken))
            throw new ArgumentException("Reset token is required", nameof(resetToken));

        var template = _emailBuilder.BuildPasswordResetEmailTemplate(email, resetToken, firstName);
        var body = _emailBuilder.GenerateEmailBody(template);
        
        var success = await _emailSender.SendEmailAsync(email, template.Title, body, isHtml: true);
        
        if (!success)
        {
            _logger.LogError("Failed to send password reset email to {Email}", email);
            throw new InvalidOperationException($"Failed to send password reset email to {email}");
        }

        _logger.LogInformation("Successfully sent password reset email to {Email}", email);
    }

    public async Task SendEmployeeWelcomeEmailAsync(string email, string password, string firstName)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required", nameof(password));

        var template = _emailBuilder.BuildEmployeeWelcomeEmailTemplate(email, password, firstName);
        var body = _emailBuilder.GenerateEmailBody(template);
        
        var success = await _emailSender.SendEmailAsync(email, template.Title, body, isHtml: true);
        
        if (!success)
        {
            _logger.LogError("Failed to send employee welcome email to {Email}", email);
            throw new InvalidOperationException($"Failed to send employee welcome email to {email}");
        }

        _logger.LogInformation("Successfully sent employee welcome email to {Email}", email);
    }
}
