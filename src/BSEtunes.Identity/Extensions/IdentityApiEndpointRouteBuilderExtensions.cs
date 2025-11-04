using BSEtunes.Identity.DTOs;
using BSEtunes.Identity.Models;
using BSEtunes.Identity.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BSEtunes.Identity.Extensions
{
    public static class IdentityApiEndpointRouteBuilderExtensions
    {
        // Reuse a single JwtSecurityTokenHandler instance to reduce allocations.
        private static readonly JwtSecurityTokenHandler s_jwtHandler = new();

        public static void MapBSEIdentityApi(this IEndpointRouteBuilder endpoints)
        {
            ArgumentNullException.ThrowIfNull(endpoints);

            var assemblyName = typeof(IdentityApiEndpointRouteBuilderExtensions)
                .Assembly
                .GetName()
                .Name ?? "Identity";

            var routeGroup = endpoints.MapGroup("")
                .WithTags(assemblyName);

            routeGroup.MapPost("/login", async ([FromBody] LoginRequestDto login, [FromServices] IServiceProvider sp) =>
            {
                if (login is null || string.IsNullOrWhiteSpace(login.Email) || string.IsNullOrWhiteSpace(login.Password)) return Results.BadRequest();

                var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var config = sp.GetRequiredService<IConfiguration>();
                var tokenStore = sp.GetRequiredService<IRefreshTokenRepository>();

                var user = await userManager.FindByNameAsync(login.Email);
                if (user == null || !await userManager.CheckPasswordAsync(user, login.Password))
                    return Results.Unauthorized();

                var tokenResponse = await CreateAndSaveTokensAsync(user, userManager, tokenStore, config);
                return tokenResponse.IsProblem
                    ? Results.Problem(tokenResponse.ProblemDetail!, statusCode: StatusCodes.Status500InternalServerError)
                    : Results.Ok(tokenResponse.TokenDto!);
            })
            .WithName("Login")
            .WithOpenApi(operation =>
            {
                operation.Summary = "User Login";
                operation.Description = "Authenticates a user and returns JWT access and refresh tokens.";
                return operation;
            });

            routeGroup.MapGet("/refresh", async ([FromBody] RefreshRequestDto refresh, [FromServices] IServiceProvider sp) =>
            {
                if (refresh is null || string.IsNullOrWhiteSpace(refresh.UserId) || string.IsNullOrWhiteSpace(refresh.RefreshToken)) return Results.BadRequest();

                var userManager = sp.GetRequiredService<UserManager<ApplicationUser>>();
                var tokenStore = sp.GetRequiredService<IRefreshTokenRepository>();
                var config = sp.GetRequiredService<IConfiguration>();

                var user = await userManager.FindByIdAsync(refresh.UserId);
                if (user == null)
                    return Results.Unauthorized();

                var storedToken = await tokenStore.ValidateRefreshTokenAsync(refresh.RefreshToken);
                if (storedToken == null || storedToken.UserId != user.Id || storedToken.ExpiresAt < DateTime.UtcNow)
                    return Results.Unauthorized();

                var tokenResponse = await CreateAndSaveTokensAsync(user, userManager, tokenStore, config);
                if (tokenResponse.IsProblem)
                    return Results.Problem(tokenResponse.ProblemDetail!, statusCode: StatusCodes.Status500InternalServerError);

                // Revoke the old refresh token after successfully issuing a new one.
                await tokenStore.RevokeTokenAsync(storedToken.Token);

                return Results.Ok(tokenResponse.TokenDto!);
            })
            .WithName("Refresh")
            .WithOpenApi(operation => {
                operation.Summary = "Refresh Tokens";
                operation.Description = "Validates a refresh token and issues new JWT access and refresh tokens.";
                return operation;
            });
        }

        private static async Task<TokenBuildResult?> CreateAndSaveTokensAsync(
            ApplicationUser user,
            UserManager<ApplicationUser> userManager,
            IRefreshTokenRepository tokenStore,
            IConfiguration config)
        {
            // read and validate JWT configuration once
            var jwtKey = config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                return new TokenBuildResult
                {
                    IsProblem = true,
                    ProblemDetail = "JWT key is not configured."
                };
            }

            var issuer = config["Jwt:Issuer"];
            var audience = config["Jwt:Audience"];

            // Build claims efficiently
            var roles = await userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r));
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            }.Union(roleClaims);

            // Create signing credentials
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var key = new SymmetricSecurityKey(keyBytes);
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Use UTC consistently
            var now = DateTime.UtcNow;
            var expiresAt = now.AddMinutes(60);

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expiresAt,
                signingCredentials: creds);

            var accessTokenString = s_jwtHandler.WriteToken(jwt);

            // Generate refresh token
            var newRefreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var refreshEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await tokenStore.SaveAsync(refreshEntity);

            var expiresIn = (int)Math.Max(0, (expiresAt - DateTime.UtcNow).TotalSeconds);

            var dto = new TokenResponseDto
            {
                AccessToken = accessTokenString,
                RefreshToken = newRefreshToken,
                TokenType = "Bearer",
                Expires = expiresIn
            };

            return new TokenBuildResult
            {
                IsProblem = false,
                TokenDto = dto
            };
        }

        private class TokenBuildResult
        {
            public bool IsProblem { get; init; }
            public string? ProblemDetail { get; init; }
            public TokenResponseDto? TokenDto { get; init; }
        }
    }
}