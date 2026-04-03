using HRS.API.Contracts.DTOs;
using HRS.API.Contracts.DTOs.Email;
using HRS.API.Services.Interfaces;
using HRS.Shared.Core.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRS.API.Controllers;

[ApiController]
[Route("api/email")]
[Authorize(Policy = "write:email")]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;

    public EmailController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendEmail([FromBody] SendEmailRequestDto request)
    {
        await _emailService.SendEmailAsync(
            request.Recipient,
            request.Subject ?? "No Subject",
            request.Body ?? string.Empty,
            request.IsBodyHtml
        );
        return Ok(ApiResponse<string>.OkResponse(null, "Email sent successfully"));
    }

    [HttpPost("send-verification")]
    public async Task<IActionResult> SendVerificationEmail([FromBody] SendVerificationEmailRequestDto request)
    {
        await _emailService.SendVerificationEmailAsync(
            request.Email,
            request.VerificationToken,
            request.FirstName
        );
        return Ok(ApiResponse<string>.OkResponse(null, "Verification email sent successfully"));
    }

    [HttpPost("send-password-reset")]
    public async Task<IActionResult> SendPasswordResetEmail([FromBody] SendPasswordResetEmailRequestDto request)
    {
        await _emailService.SendPasswordResetEmailAsync(
            request.Email,
            request.ResetToken,
            request.FirstName
        );
        return Ok(ApiResponse<string>.OkResponse(null, "Password reset email sent successfully"));
    }

    [HttpPost("send-employee-welcome")]
    public async Task<IActionResult> SendEmployeeWelcomeEmail([FromBody] SendEmployeeWelcomeEmailRequestDto request)
    {
        await _emailService.SendEmployeeWelcomeEmailAsync(
            request.Email,
            request.Password,
            request.FirstName
        );
        return Ok(ApiResponse<string>.OkResponse(null, "Employee welcome email sent successfully"));
    }
}
