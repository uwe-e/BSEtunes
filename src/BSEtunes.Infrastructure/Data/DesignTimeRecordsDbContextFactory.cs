using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.UserSecrets;
using MySqlConnector;
using System.IO;

namespace BSEtunes.Infrastructure.Data;
public class DesignTimeRecordsDbContextFactory : IDesignTimeDbContextFactory<RecordsDbContext>
{
    public RecordsDbContext CreateDbContext(string[] args)
    {
        // Adjust the path to point to the API project folder if needed
        var basePath = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "..","src", "BSEtunes.Api"));
        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddUserSecrets("c211a062-3e9e-4504-888a-d24b2c172e95")
            .Build();

        var connectionStringBuilder = new MySqlConnectionStringBuilder
        {
            Server = config["mysql:server"],
            Database = config["mysql:database"],
            UserID = config["mysql:userid"],
            Password = config["mysql:password"]
        };

        var conn = config.GetConnectionString("RecordsDb");
        var optionsBuilder = new DbContextOptionsBuilder<RecordsDbContext>();
        optionsBuilder.UseMySql(connectionStringBuilder.ConnectionString, ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString));

        return new RecordsDbContext(optionsBuilder.Options);
    }
}