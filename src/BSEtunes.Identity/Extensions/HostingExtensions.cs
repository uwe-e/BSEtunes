using BSEtunes.Identity.Data;
using BSEtunes.Identity.Models;
using BSEtunes.Identity.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace BSEtunes.Identity.Extensions
{
    public static class HostingExtensions
    {
        public static WebApplicationBuilder ConfigureBSEIdentity(this WebApplicationBuilder builder)
        {
            var connectionStringBuilder = new MySqlConnectionStringBuilder
            {
                Server = builder.Configuration["mysqlidentity:server"],
                Database = builder.Configuration["mysqlidentity:database"],
                UserID = builder.Configuration["mysqlidentity:userid"],
                Password = builder.Configuration["mysqlidentity:password"]
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
            //builder.Services.AddAuthorization();

            return builder;
        }
    }
}
