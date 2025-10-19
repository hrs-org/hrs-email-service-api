namespace HRS.API.Contracts.DTOs.Email;

public class SendEmailRequestDto
{
    public string Recipient { get; set; } = null!;
    public string? Subject { get; set; }
    public string? Body { get; set; }
    public bool IsBodyHtml { get; set; } = true;
}
