using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OrderSystem.Common;
using OrderSystem.Infrastructure.Options;
using OrderSystem.Middlewares;

namespace OrderSystem.Extensions
{
    public static class ApiServiceExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Controllers
            services.AddControllers()
                .AddJsonOptions(opt =>
                    opt.JsonSerializerOptions.Converters.Add(
                        new System.Text.Json.Serialization.JsonStringEnumConverter()))
                .ConfigureApiBehaviorOptions(opt =>
                {
                    // Map DataAnnotation model-state errors to the shared ApiResponse envelope
                    opt.InvalidModelStateResponseFactory = ctx =>
                    {
                        var errors = ctx.ModelState
                            .Where(e => e.Value?.Errors.Count > 0)
                            .SelectMany(e => e.Value!.Errors.Select(err => $"{e.Key}: {err.ErrorMessage}"))
                            .ToList();

                        return new BadRequestObjectResult(
                            ApiResponse.Fail("Validation failed.", StatusCodes.Status400BadRequest, errors));
                    };
                });

            // Exception handling
            services.AddExceptionHandler<GlobalExceptionHandler>();

            // OpenAPI
            services.AddOpenApi();

            // Localization
            services.AddLocalization(opt => opt.ResourcesPath = "Resources");

            // JWT Authentication
            services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
            var jwt = configuration.GetSection("Jwt");

            services.AddAuthentication(opt =>
                    {
                        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    })
                    .AddJwtBearer(opt =>
                    {
                        opt.SaveToken = true;
                        opt.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer           = true,
                            ValidIssuer              = jwt["Issuer"],
                            ValidateAudience         = true,
                            ValidAudience            = jwt["Audience"],
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Secret"]!)),
                            ValidateLifetime         = true,
                            ClockSkew                = TimeSpan.Zero,
                        };
                    });

            services.AddAuthorization();

            return services;
        }
    }
}
