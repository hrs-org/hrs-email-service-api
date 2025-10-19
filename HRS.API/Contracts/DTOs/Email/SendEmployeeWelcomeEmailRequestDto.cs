namespace HRS.API.Contracts.DTOs.Email;

public class SendEmployeeWelcomeEmailRequestDto
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
}
