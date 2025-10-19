namespace HRS.API.Contracts.DTOs.Email;

public class SendVerificationEmailRequestDto
{
    public string Email { get; set; } = null!;
    public string VerificationToken { get; set; } = null!;
    public string FirstName { get; set; } = null!;
}
