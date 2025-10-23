using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.API.Models;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace HRS.API.Tests.Services;

public class EmailServiceTests
{
    private readonly IEmailSenderService _emailSenderMock;
    private readonly IEmailBuilderService _emailBuilderMock;
    private readonly ILogger<EmailService> _loggerMock;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _emailSenderMock = Substitute.For<IEmailSenderService>();
        _emailBuilderMock = Substitute.For<IEmailBuilderService>();
        _loggerMock = Substitute.For<ILogger<EmailService>>();
        _emailService = new EmailService(_emailSenderMock, _emailBuilderMock, _loggerMock);
    }

    #region SendEmailAsync Tests

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task SendEmailAsync_InvalidRecipient_ThrowsArgumentException(string? recipient)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendEmailAsync(recipient!, "Subject", "Body"));

        Assert.Equal("recipient", exception.ParamName);
    }

    [Fact]
    public async Task SendEmailAsync_NullRecipient_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendEmailAsync(null!, "Subject", "Body"));

        Assert.Equal("recipient", exception.ParamName);
    }

    [Fact]
    public async Task SendEmailAsync_Success_LogsInformation()
    {
        _emailSenderMock.SendEmailAsync("test@test.com", "Subject", "Body", true)
            .Returns(true);

        await _emailService.SendEmailAsync("test@test.com", "Subject", "Body");

        await _emailSenderMock.Received(1).SendEmailAsync("test@test.com", "Subject", "Body", true);
        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully sent email to test@test.com")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendEmailAsync_Failure_LogsErrorAndThrowsException()
    {
        _emailSenderMock.SendEmailAsync("test@test.com", "Subject", "Body", false)
            .Returns(false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _emailService.SendEmailAsync("test@test.com", "Subject", "Body", false));

        Assert.Contains("Failed to send email to test@test.com", exception.Message);
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Failed to send simple email to test@test.com")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region SendVerificationEmailAsync Tests

    [Theory]
    [InlineData("", "token", "Email is required", "email")]
    [InlineData("test@test.com", "", "Verification token is required", "verificationToken")]
    public async Task SendVerificationEmailAsync_InvalidParameters_ThrowsArgumentException(
        string? email, string? token, string expectedMessage, string expectedParamName)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendVerificationEmailAsync(email!, token!, "John"));

        Assert.Contains(expectedMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_NullEmail_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendVerificationEmailAsync(null!, "token", "John"));

        Assert.Contains("Email is required", exception.Message);
        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_NullToken_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendVerificationEmailAsync("test@test.com", null!, "John"));

        Assert.Contains("Verification token is required", exception.Message);
        Assert.Equal("verificationToken", exception.ParamName);
    }

    [Fact]
    public async Task SendVerificationEmailAsync_Success_LogsInformation()
    {
        var template = new EmailTemplate { Title = "Verify Email" };
        _emailBuilderMock.BuildVerificationEmailTemplate("test@test.com", "token123", "John")
            .Returns(template);
        _emailBuilderMock.GenerateEmailBody(template)
            .Returns("<html>Body</html>");
        _emailSenderMock.SendEmailAsync("test@test.com", "Verify Email", "<html>Body</html>", true)
            .Returns(true);

        await _emailService.SendVerificationEmailAsync("test@test.com", "token123", "John");

        await _emailSenderMock.Received(1).SendEmailAsync("test@test.com", "Verify Email", "<html>Body</html>", true);
        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully sent verification email to test@test.com")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendVerificationEmailAsync_Failure_LogsErrorAndThrowsException()
    {
        var template = new EmailTemplate { Title = "Verify Email" };
        _emailBuilderMock.BuildVerificationEmailTemplate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(template);
        _emailBuilderMock.GenerateEmailBody(Arg.Any<EmailTemplate>())
            .Returns("<html>Body</html>");
        _emailSenderMock.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), true)
            .Returns(false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _emailService.SendVerificationEmailAsync("test@test.com", "token123", "John"));

        Assert.Contains("Failed to send verification email to test@test.com", exception.Message);
        _loggerMock.Received(1).Log(
            LogLevel.Error,
            Arg.Any<EventId>(),
            Arg.Any<object>(),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    #endregion

    #region SendPasswordResetEmailAsync Tests

    [Theory]
    [InlineData("", "token", "Email is required", "email")]
    [InlineData("test@test.com", "", "Reset token is required", "resetToken")]
    public async Task SendPasswordResetEmailAsync_InvalidParameters_ThrowsArgumentException(
        string? email, string? token, string expectedMessage, string expectedParamName)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendPasswordResetEmailAsync(email!, token!, "John"));

        Assert.Contains(expectedMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_NullEmail_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendPasswordResetEmailAsync(null!, "token", "John"));

        Assert.Contains("Email is required", exception.Message);
        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_NullToken_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendPasswordResetEmailAsync("test@test.com", null!, "John"));

        Assert.Contains("Reset token is required", exception.Message);
        Assert.Equal("resetToken", exception.ParamName);
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_Success_LogsInformation()
    {
        var template = new EmailTemplate { Title = "Reset Password" };
        _emailBuilderMock.BuildPasswordResetEmailTemplate("test@test.com", "resetToken", "John")
            .Returns(template);
        _emailBuilderMock.GenerateEmailBody(template)
            .Returns("<html>Reset</html>");
        _emailSenderMock.SendEmailAsync("test@test.com", "Reset Password", "<html>Reset</html>", true)
            .Returns(true);

        await _emailService.SendPasswordResetEmailAsync("test@test.com", "resetToken", "John");

        await _emailSenderMock.Received(1).SendEmailAsync("test@test.com", "Reset Password", "<html>Reset</html>", true);
        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully sent password reset email to test@test.com")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendPasswordResetEmailAsync_Failure_LogsErrorAndThrowsException()
    {
        var template = new EmailTemplate { Title = "Reset Password" };
        _emailBuilderMock.BuildPasswordResetEmailTemplate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(template);
        _emailBuilderMock.GenerateEmailBody(Arg.Any<EmailTemplate>())
            .Returns("<html>Reset</html>");
        _emailSenderMock.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), true)
            .Returns(false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _emailService.SendPasswordResetEmailAsync("test@test.com", "resetToken", "John"));

        Assert.Contains("Failed to send password reset email to test@test.com", exception.Message);
    }

    #endregion

    #region SendEmployeeWelcomeEmailAsync Tests

    [Theory]
    [InlineData("", "password", "Email is required", "email")]
    [InlineData("test@test.com", "", "Password is required", "password")]
    public async Task SendEmployeeWelcomeEmailAsync_InvalidParameters_ThrowsArgumentException(
        string? email, string? password, string expectedMessage, string expectedParamName)
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendEmployeeWelcomeEmailAsync(email!, password!, "John"));

        Assert.Contains(expectedMessage, exception.Message);
        Assert.Equal(expectedParamName, exception.ParamName);
    }

    [Fact]
    public async Task SendEmployeeWelcomeEmailAsync_NullEmail_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendEmployeeWelcomeEmailAsync(null!, "password", "John"));

        Assert.Contains("Email is required", exception.Message);
        Assert.Equal("email", exception.ParamName);
    }

    [Fact]
    public async Task SendEmployeeWelcomeEmailAsync_NullPassword_ThrowsArgumentException()
    {
        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => _emailService.SendEmployeeWelcomeEmailAsync("test@test.com", null!, "John"));

        Assert.Contains("Password is required", exception.Message);
        Assert.Equal("password", exception.ParamName);
    }

    [Fact]
    public async Task SendEmployeeWelcomeEmailAsync_Success_LogsInformation()
    {
        var template = new EmailTemplate { Title = "Welcome" };
        _emailBuilderMock.BuildEmployeeWelcomeEmailTemplate("test@test.com", "pass123", "John")
            .Returns(template);
        _emailBuilderMock.GenerateEmailBody(template)
            .Returns("<html>Welcome</html>");
        _emailSenderMock.SendEmailAsync("test@test.com", "Welcome", "<html>Welcome</html>", true)
            .Returns(true);

        await _emailService.SendEmployeeWelcomeEmailAsync("test@test.com", "pass123", "John");

        await _emailSenderMock.Received(1).SendEmailAsync("test@test.com", "Welcome", "<html>Welcome</html>", true);
        _loggerMock.Received(1).Log(
            LogLevel.Information,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("Successfully sent employee welcome email to test@test.com")),
            null,
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public async Task SendEmployeeWelcomeEmailAsync_Failure_LogsErrorAndThrowsException()
    {
        var template = new EmailTemplate { Title = "Welcome" };
        _emailBuilderMock.BuildEmployeeWelcomeEmailTemplate(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
            .Returns(template);
        _emailBuilderMock.GenerateEmailBody(Arg.Any<EmailTemplate>())
            .Returns("<html>Welcome</html>");
        _emailSenderMock.SendEmailAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), true)
            .Returns(false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => _emailService.SendEmployeeWelcomeEmailAsync("test@test.com", "pass123", "John"));

        Assert.Contains("Failed to send employee welcome email to test@test.com", exception.Message);
    }

    #endregion
}
