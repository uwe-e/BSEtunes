using BSEtunes.Identity.Data;
using BSEtunes.Identity.Models;
using BSEtunes.Identity.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using System.Text;

namespace BSEtunes.Identity.Extensions
{
    public static class HostingExtensions
    {
        public static WebApplicationBuilder ConfigureBSEIdentity(this WebApplicationBuilder builder)
        {
            var config = builder.Configuration;

            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = builder.Configuration["identity:backend:server"],
                Port = uint.Parse(builder.Configuration["identity:backend:port"] ?? "3306"),
                Database = builder.Configuration["identity:backend:database"],
                UserID = builder.Configuration["identity:backend:userid"],
                Password = builder.Configuration["identity:backend:password"]
            };

            builder.Services.AddDbContext<IdentityDbContext>(options =>
            {
                options.UseMySql(connectionStringBuilder.ConnectionString,
                    ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString));
            });

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["Jwt:Issuer"],
                    ValidAudience = config["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["Jwt:Key"]))
                };
            });

            builder.Services.AddAuthorization();

            return builder;
        }
    }
}
