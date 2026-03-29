using System.Security.Claims;
using HRS.API.Filters;
using HRS.API.Middleware;
using HRS.API.Services;
using HRS.API.Services.Interfaces;
using HRS.Shared.Core.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IEmailBuilderService, EmailBuilderService>();
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAppConfiguration, AppConfiguration>();
builder.Services.AddHttpContextAccessor();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddControllers(options => { options.Filters.Add<ValidationFilter>(); });


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "HRS API", Version = "v1" });

    // 🔑 Enable JWT Bearer in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer' followed by your JWT token.\n\nExample: **Bearer eyJhbGciOi...**"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});



builder.Services.AddAutoMapper(cfg => { }, typeof(Program));

var auth0Domain = builder.Configuration["Auth0:Domain"]!;
var auth0Audience = builder.Configuration["Auth0:Audience"]!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{auth0Domain}/";
        options.Audience = auth0Audience;
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = $"https://{auth0Domain}/",
            ValidAudience = auth0Audience,
            NameClaimType = "sub"
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var claims = context.Principal?.Claims.ToList() ?? new List<Claim>();
                var subClaim = claims.FirstOrDefault(c => c.Type == "sub");
                if (subClaim != null && !claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
                {
                    var claimsIdentity = (ClaimsIdentity)context.Principal?.Identity!;
                    claimsIdentity.AddClaim(new Claim(ClaimTypes.NameIdentifier, subClaim.Value));
                }
                await Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    // Email service scopes
    options.AddPolicy("write:email", policy =>
        policy.Requirements.Add(new ScopeRequirement("write:email", auth0Domain)));
});

builder.Services.AddSingleton<IAuthorizationHandler, ScopeHandler>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowWebClient", policy =>
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"]?.Split(',') ?? Array.Empty<string>()
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
    );
});

// Add health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "HRS API v1");
    c.RoutePrefix = "swagger";
});

//app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AllowWebClient");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Map health check endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/live");
app.MapHealthChecks("/health/ready");

app.Run();

