namespace HRS.API.DTOs;

public class SendEmailRequest
{
    public string Recipient { get; set; } = null!;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsBodyHtml { get; set; } = true;
}

public class SendVerificationEmailRequest
{
    public string Email { get; set; } = null!;
    public string VerificationToken { get; set; } = null!;
    public string FirstName { get; set; } = null!;
}

public class SendPasswordResetEmailRequest
{
    public string Email { get; set; } = null!;
    public string ResetToken { get; set; } = null!;
    public string FirstName { get; set; } = null!;
}

public class SendEmployeeWelcomeEmailRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
}