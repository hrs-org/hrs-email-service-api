using HRS.API.DTOs;
using HRS.API.Services.Interfaces;

namespace HRS.API.Endpoints;

public static class EmailEndpoints
{
    public static void MapEmailEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/email");

        group.MapPost("/send", async (SendEmailRequest request, IEmailSenderService emailService) =>
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Recipient))
                return Results.BadRequest("Recipient is required.");

            try
            {
                var success = await emailService.SendEmailAsync(
                    request.Recipient,
                    request.Subject ?? "No Subject",
                    request.Body ?? string.Empty,
                    request.IsBodyHtml
                );
                
                return success 
                    ? Results.Ok(new { message = "Email sent successfully" })
                    : Results.Problem("Failed to send email", statusCode: 500);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("SendEmail")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/send-verification", async (
            SendVerificationEmailRequest request, 
            IEmailBuilderService emailBuilder,
            IEmailSenderService emailSender) =>
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest("Email is required.");

            try
            {
                var template = emailBuilder.BuildVerificationEmailTemplate(
                    request.Email,
                    request.VerificationToken,
                    request.FirstName
                );

                var body = emailBuilder.GenerateEmailBody(template);

                var success = await emailSender.SendEmailAsync(
                    request.Email,
                    template.Title,
                    body,
                    isHtml: true
                );

                return success
                    ? Results.Ok(new { message = "Verification email sent successfully" })
                    : Results.Problem("Failed to send verification email", statusCode: 500);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("SendVerificationEmail")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/send-password-reset", async (
            SendPasswordResetEmailRequest request,
            IEmailBuilderService emailBuilder,
            IEmailSenderService emailSender) =>
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest("Email is required.");

            try
            {
                var template = emailBuilder.BuildPasswordResetEmailTemplate(
                    request.Email,
                    request.ResetToken,
                    request.FirstName
                );

                var body = emailBuilder.GenerateEmailBody(template);

                var success = await emailSender.SendEmailAsync(
                    request.Email,
                    template.Title,
                    body,
                    isHtml: true
                );

                return success
                    ? Results.Ok(new { message = "Password reset email sent successfully" })
                    : Results.Problem("Failed to send password reset email", statusCode: 500);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("SendPasswordResetEmail")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);

        group.MapPost("/send-employee-welcome", async (
            SendEmployeeWelcomeEmailRequest request,
            IEmailBuilderService emailBuilder,
            IEmailSenderService emailSender) =>
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Email))
                return Results.BadRequest("Email is required.");

            try
            {
                var template = emailBuilder.BuildEmployeeWelcomeEmailTemplate(
                    request.Email,
                    request.Password,
                    request.FirstName
                );

                var body = emailBuilder.GenerateEmailBody(template);

                var success = await emailSender.SendEmailAsync(
                    request.Email,
                    template.Title,
                    body,
                    isHtml: true
                );

                return success
                    ? Results.Ok(new { message = "Employee welcome email sent successfully" })
                    : Results.Problem("Failed to send employee welcome email", statusCode: 500);
            }
            catch (Exception ex)
            {
                return Results.Problem(detail: ex.Message, statusCode: 500);
            }
        })
        .WithName("SendEmployeeWelcomeEmail")
        .Produces<object>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status500InternalServerError);
    }
}