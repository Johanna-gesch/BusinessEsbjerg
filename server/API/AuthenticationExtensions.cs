using System.Text;
using Infra.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using NSwag;
using NSwag.Generation.Processors.Security;
using Service;
using Service.Security;


namespace API;

public static class AuthenticationExtensions
{
    public static IServiceCollection ConfigureDomainServices(
        this IServiceCollection services,
        string jwtSecret)
    {
        services.AddScoped<IPasswordHasher, Argon2PasswordHasher>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<UserService>();
        services.AddScoped<IUserRepo, UserRepo>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "security-api",

                    ValidateAudience = true,
                    ValidAudience = "security-api",

                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtSecret))
                };
            });

        services.AddOpenApiDocument(settings =>
        {
            settings.SchemaSettings.SchemaProcessors.Add(
                new RequireNotNullableSchemaProcessor());

            settings.AddSecurity("JWT", Enumerable.Empty<string>(), new OpenApiSecurityScheme
            {
                Type = OpenApiSecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Text: Bearer {your JWT-token}"
            });
            settings.OperationProcessors.Add(
                new AspNetCoreOperationSecurityScopeProcessor("JWT"));
        });
        services.AddAuthorization();

        return services;

    }
}