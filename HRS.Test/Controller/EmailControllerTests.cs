using HRS.API.Controllers;
using HRS.API.Contracts.DTOs.Email;
using HRS.API.Services.Interfaces;
using HRS.Shared.Core.Dtos;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace HRS.API.Tests.Controllers;

public class EmailControllerTests
{
    private readonly MockEmailService _emailServiceMock;
    private readonly EmailController _controller;

    public EmailControllerTests()
    {
        _emailServiceMock = new MockEmailService();
        _controller = new EmailController(_emailServiceMock);
    }

    #region SendEmail Tests

    [Fact]
    public async Task SendEmail_ValidRequest_ReturnsOkWithSuccessMessage()
    {
        var request = new SendEmailRequestDto
        {
            Recipient = "test@test.com",
            Subject = "Test Subject",
            Body = "Test Body",
            IsBodyHtml = true
        };

        var result = await _controller.SendEmail(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Email sent successfully", response.Message);
        Assert.Equal("test@test.com", _emailServiceMock.LastRecipient);
        Assert.Equal("Test Subject", _emailServiceMock.LastSubject);
        Assert.Equal("Test Body", _emailServiceMock.LastBody);
        Assert.True(_emailServiceMock.LastIsHtml);
    }

    [Fact]
    public async Task SendEmail_NullSubject_UsesDefaultSubject()
    {
        var request = new SendEmailRequestDto
        {
            Recipient = "test@test.com",
            Subject = null,
            Body = "Test Body",
            IsBodyHtml = false
        };

        var result = await _controller.SendEmail(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("No Subject", _emailServiceMock.LastSubject);
    }

    [Fact]
    public async Task SendEmail_NullBody_UsesEmptyString()
    {
        var request = new SendEmailRequestDto
        {
            Recipient = "test@test.com",
            Subject = "Test",
            Body = null,
            IsBodyHtml = true
        };

        var result = await _controller.SendEmail(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(string.Empty, _emailServiceMock.LastBody);
    }

    [Fact]
    public async Task SendEmail_ServiceThrowsException_PropagatesException()
    {
        _emailServiceMock.ShouldThrowException = true;
        var request = new SendEmailRequestDto
        {
            Recipient = "test@test.com",
            Subject = "Test",
            Body = "Body"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.SendEmail(request));
    }

    #endregion

    #region SendVerificationEmail Tests

    [Fact]
    public async Task SendVerificationEmail_ValidRequest_ReturnsOkWithSuccessMessage()
    {
        var request = new SendVerificationEmailRequestDto
        {
            Email = "test@test.com",
            VerificationToken = "token123",
            FirstName = "John"
        };

        var result = await _controller.SendVerificationEmail(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Verification email sent successfully", response.Message);
        Assert.Equal("test@test.com", _emailServiceMock.LastVerificationEmail);
        Assert.Equal("token123", _emailServiceMock.LastVerificationToken);
        Assert.Equal("John", _emailServiceMock.LastVerificationFirstName);
    }

    [Fact]
    public async Task SendVerificationEmail_ServiceThrowsException_PropagatesException()
    {
        _emailServiceMock.ShouldThrowException = true;
        var request = new SendVerificationEmailRequestDto
        {
            Email = "test@test.com",
            VerificationToken = "token",
            FirstName = "John"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.SendVerificationEmail(request));
    }

    #endregion

    #region SendPasswordResetEmail Tests

    [Fact]
    public async Task SendPasswordResetEmail_ValidRequest_ReturnsOkWithSuccessMessage()
    {
        var request = new SendPasswordResetEmailRequestDto
        {
            Email = "test@test.com",
            ResetToken = "resetToken123",
            FirstName = "Jane"
        };

        var result = await _controller.SendPasswordResetEmail(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Password reset email sent successfully", response.Message);
        Assert.Equal("test@test.com", _emailServiceMock.LastPasswordResetEmail);
        Assert.Equal("resetToken123", _emailServiceMock.LastPasswordResetToken);
        Assert.Equal("Jane", _emailServiceMock.LastPasswordResetFirstName);
    }

    [Fact]
    public async Task SendPasswordResetEmail_ServiceThrowsException_PropagatesException()
    {
        _emailServiceMock.ShouldThrowException = true;
        var request = new SendPasswordResetEmailRequestDto
        {
            Email = "test@test.com",
            ResetToken = "token",
            FirstName = "Jane"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.SendPasswordResetEmail(request));
    }

    #endregion

    #region SendEmployeeWelcomeEmail Tests

    [Fact]
    public async Task SendEmployeeWelcomeEmail_ValidRequest_ReturnsOkWithSuccessMessage()
    {
        var request = new SendEmployeeWelcomeEmailRequestDto
        {
            Email = "employee@test.com",
            Password = "TempPass123",
            FirstName = "Bob"
        };

        var result = await _controller.SendEmployeeWelcomeEmail(request);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<string>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Employee welcome email sent successfully", response.Message);
        Assert.Equal("employee@test.com", _emailServiceMock.LastEmployeeWelcomeEmail);
        Assert.Equal("TempPass123", _emailServiceMock.LastEmployeeWelcomePassword);
        Assert.Equal("Bob", _emailServiceMock.LastEmployeeWelcomeFirstName);
    }

    [Fact]
    public async Task SendEmployeeWelcomeEmail_ServiceThrowsException_PropagatesException()
    {
        _emailServiceMock.ShouldThrowException = true;
        var request = new SendEmployeeWelcomeEmailRequestDto
        {
            Email = "test@test.com",
            Password = "pass",
            FirstName = "Bob"
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _controller.SendEmployeeWelcomeEmail(request));
    }

    #endregion

    #region Mock Class

    private class MockEmailService : IEmailService
    {
        public bool ShouldThrowException { get; set; }

        // SendEmailAsync captured values
        public string? LastRecipient { get; private set; }
        public string? LastSubject { get; private set; }
        public string? LastBody { get; private set; }
        public bool LastIsHtml { get; private set; }

        // SendVerificationEmailAsync captured values
        public string? LastVerificationEmail { get; private set; }
        public string? LastVerificationToken { get; private set; }
        public string? LastVerificationFirstName { get; private set; }

        // SendPasswordResetEmailAsync captured values
        public string? LastPasswordResetEmail { get; private set; }
        public string? LastPasswordResetToken { get; private set; }
        public string? LastPasswordResetFirstName { get; private set; }

        // SendEmployeeWelcomeEmailAsync captured values
        public string? LastEmployeeWelcomeEmail { get; private set; }
        public string? LastEmployeeWelcomePassword { get; private set; }
        public string? LastEmployeeWelcomeFirstName { get; private set; }

        public Task SendEmailAsync(string recipient, string subject, string body, bool isHtml = true)
        {
            if (ShouldThrowException)
                throw new InvalidOperationException("Email service error");

            LastRecipient = recipient;
            LastSubject = subject;
            LastBody = body;
            LastIsHtml = isHtml;
            return Task.CompletedTask;
        }

        public Task SendVerificationEmailAsync(string email, string verificationToken, string firstName)
        {
            if (ShouldThrowException)
                throw new InvalidOperationException("Email service error");

            LastVerificationEmail = email;
            LastVerificationToken = verificationToken;
            LastVerificationFirstName = firstName;
            return Task.CompletedTask;
        }

        public Task SendPasswordResetEmailAsync(string email, string resetToken, string firstName)
        {
            if (ShouldThrowException)
                throw new InvalidOperationException("Email service error");

            LastPasswordResetEmail = email;
            LastPasswordResetToken = resetToken;
            LastPasswordResetFirstName = firstName;
            return Task.CompletedTask;
        }

        public Task SendEmployeeWelcomeEmailAsync(string email, string password, string firstName)
        {
            if (ShouldThrowException)
                throw new InvalidOperationException("Email service error");

            LastEmployeeWelcomeEmail = email;
            LastEmployeeWelcomePassword = password;
            LastEmployeeWelcomeFirstName = firstName;
            return Task.CompletedTask;
        }
    }

    #endregion
}
