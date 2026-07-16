using Api;
using Api.Authorization;
using Api.Authentication;
using Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using FluentValidation;
using Api.Validators.Auth;
using Core.UseCases.Auth.Commands;
using Microsoft.AspNetCore.Mvc;
using Api.Dtos.Responses;
using Api.Mappers;
using Core.Entities.Constants;
using Infrastructure.Options;
using Microsoft.AspNetCore.Authorization;

var currentDirectory = Directory.GetCurrentDirectory();
var envPath = Path.Combine(currentDirectory, ".env");

if (!File.Exists(envPath))
{
    envPath = Path.GetFullPath(Path.Combine(currentDirectory, "..", ".env"));
}

if (File.Exists(envPath))
{
    DotNetEnv.Env.Load(envPath);
}

var builder = WebApplication.CreateBuilder(args);
var jwtOptions = JwtOptions.FromConfiguration(builder.Configuration);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddMediatR(config =>
    config.RegisterServicesFromAssemblyContaining<LoginCommand>());
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
        new BadRequestObjectResult(ApiErrorResponseDto.Validation(
            ValidationErrorMapper.FromModelState(context.ModelState)));
});
builder.Services.AddValidatorsFromAssemblyContaining<LoginRequestDtoValidator>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

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

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddScoped<CurrentAdminContext>();
builder.Services.AddScoped<ICurrentAdminContext>(serviceProvider =>
    serviceProvider.GetRequiredService<CurrentAdminContext>());
builder.Services.AddAuthorization(options =>
{
    foreach (var permissionCode in PermissionCodes.All)
    {
        options.AddPolicy(
            permissionCode,
            policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.Requirements.Add(new PermissionRequirement(permissionCode));
            });
    }
});

var app = builder.Build();

app.UseExceptionHandler(_ => { });
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
