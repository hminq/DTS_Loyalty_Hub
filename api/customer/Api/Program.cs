using Api;
using Api.Authentication;
using Api.Validators;
using Core.UseCases.Auth.Commands;
using FluentValidation;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Api.Dtos.Responses;
using Api.Mappers;
using Infrastructure.Options;

var envPath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", ".env"));

// Load local environment variables from the Customer API root directory.
if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);
var jwtOptions = JwtOptions.FromConfiguration(builder.Configuration);

// Register infrastructure services and discover all MediatR handlers from Core.
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblyContaining<LoginCommand>());

// Configure controllers and the scoped accessor for the authenticated customer.
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentCustomerAccessor, CurrentCustomerAccessor>();

// Return the shared API validation shape for model binding and FluentValidation errors.
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(ApiErrorResponseDto.Validation(
            ValidationErrorMapper.FromModelState(context.ModelState)));
});
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Validate JWT issuer, audience, signature, and expiration for protected endpoints.
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Handle exceptions first, then authenticate and authorize before dispatching controllers.
app.UseExceptionHandler();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
