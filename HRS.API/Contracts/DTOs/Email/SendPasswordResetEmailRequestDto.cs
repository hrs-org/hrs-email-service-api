namespace HRS.API.Contracts.DTOs.Email;

public class SendPasswordResetEmailRequestDto
{
    public string Email { get; set; } = null!;
    public string ResetToken { get; set; } = null!;
    public string FirstName { get; set; } = null!;
}
