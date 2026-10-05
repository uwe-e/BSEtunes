using Azure.Identity;
using BSEtunes.Application.Mapping;
using BSEtunes.Application.Services;
using BSEtunes.Contracts.Enums;
using BSEtunes.Identity.Extensions;
using BSEtunes.Infrastructure.Configuration;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Repositories;
using BSEtunes.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using Serilog;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Log directory is anchored to the app's base directory so it resolves correctly
// regardless of the IIS working directory (which is not the app folder).
// Exposed as an environment variable so the File sink path in appsettings.json
// can reference it as %BSE_LOG_DIR% without hardcoding an absolute path in config.
//var logDir = Path.Combine(AppContext.BaseDirectory, "logs");
//Environment.SetEnvironmentVariable("BSE_LOG_DIR", logDir);

// Ensure the logs directory exists and is writable before Serilog tries to open files in it.
// If this fails, write to the Windows Application Event Log — the one sink that never needs
// file-system permissions — so the exact error is always visible.
//try
//{
//    Directory.CreateDirectory(logDir);
//    var probe = Path.Combine(logDir, ".write-test");
//    File.WriteAllText(probe, string.Empty);
//    File.Delete(probe);
//}
//catch (Exception ex)
//{
//    const string source = "BSEtunes.Api";
//    const string logName = "Application";
//    try
//    {
//        if (!System.Diagnostics.EventLog.SourceExists(source))
//            System.Diagnostics.EventLog.CreateEventSource(source, logName);
//        System.Diagnostics.EventLog.WriteEntry(source,
//            $"Cannot write to log directory '{logDir}': {ex}",
//            System.Diagnostics.EventLogEntryType.Error);
//    }
//    catch { /* Event Log also unavailable — nothing more we can do at this point */ }
//}

// Bootstrap logger — active until UseSerilog builds the real logger from configuration.
// The File sink path is still set in code here because the bootstrap logger starts
// before appsettings are loaded.
//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Warning()
//    .WriteTo.Console()
//    .WriteTo.File(
//        path: Path.Combine(logDir, "bootstrap-.log"),
//        rollingInterval: RollingInterval.Day,
//        retainedFileCountLimit: 7)
//    .CreateBootstrapLogger();

// Main logger is driven entirely by appsettings (levels, sinks, enrichers).
// The File sink in appsettings uses %BSE_LOG_DIR% which is set above.
builder.Host.UseSerilog((context, services, loggerConfig) =>
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

try
{
    Log.Information("Starting BSEtunes API");

    //if (builder.Environment.IsProduction())
    {
        var keyVaultName = builder.Configuration["KeyVault:Name"];
        var certThumbprint = builder.Configuration["KeyVault:AzureADCertThumbprint"];
        var directoryId = builder.Configuration["KeyVault:AzureADDirectoryId"];
        var applicationId = builder.Configuration["KeyVault:AzureADApplicationId"];

        if (!string.IsNullOrWhiteSpace(keyVaultName)
            && !string.IsNullOrWhiteSpace(certThumbprint)
            && !string.IsNullOrWhiteSpace(directoryId)
            && !string.IsNullOrWhiteSpace(applicationId))
        {
            try
            {
                using var x509Store = new X509Store(StoreLocation.LocalMachine);
                x509Store.Open(OpenFlags.ReadOnly);

                var x509Certificate = x509Store.Certificates
                    .Find(
                        X509FindType.FindByThumbprint,
                        certThumbprint,
                        validOnly: false)
                    .OfType<X509Certificate2>()
                    .FirstOrDefault();

                if (x509Certificate is null)
                {
                    Log.Warning("Azure AD certificate with thumbprint {Thumbprint} not found. Skipping Key Vault configuration.", certThumbprint);
                }
                else
                {
                    var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");

                    try
                    {
                        builder.Configuration.AddAzureKeyVault(
                            keyVaultUri,
                            new ClientCertificateCredential(
                                directoryId,
                                applicationId,
                                x509Certificate));
                        Log.Information("Added Azure Key Vault configuration from {KeyVault}", keyVaultName);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error adding Azure Key Vault configuration.");
                        // Intentionally not rethrowing to allow startup to continue without Key Vault
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error accessing certificate store for Key Vault configuration.");
            }
        }
        else
        {
            Log.Debug("Key Vault configuration not present or incomplete. Skipping Key Vault setup.");
        }
    }

    var connectionStringBuilder = new MySqlConnectionStringBuilder
    {
        Server = builder.Configuration["tunes:backend:server"],
        Port = uint.Parse(builder.Configuration["tunes:backend:port"] ?? "3306"),
        Database = builder.Configuration["tunes:backend:database"],
        UserID = builder.Configuration["tunes:backend:userid"],
        Password = builder.Configuration["tunes:backend:password"]
    };

    // Temporary design-time DbContext factory check that creates the DbContext and the models
    //#if DEBUG
    //try
    //{
    //    // Temporary check — safe to guard with DEBUG so it doesn't run in production
    //    var factory = new BSEtunes.Infrastructure.Data.DesignTimeRecordsDbContextFactory();
    //    using var db = factory.CreateDbContext(Array.Empty<string>());
    //    Console.WriteLine($"[DesignTimeFactory] CanConnect: {db.Database.CanConnect()}");
    //}
    //catch (Exception ex)
    //{
    //    Console.WriteLine($"[DesignTimeFactory] ERROR: {ex}");
    //    // Uncomment to prompt Visual Studio to attach when CLI invokes this:
    //    // System.Diagnostics.Debugger.Launch();
    //}
    //#endif
    builder.Services.Configure<FileShareOptions>(
    builder.Configuration.GetSection("tunes:fileshare"));

#if WINDOWS
    builder.Services.AddScoped<ImpersonatedFileAccessor>(sp =>
    {
        var options = sp.GetRequiredService<IOptions<FileShareOptions>>().Value;
        return new ImpersonatedFileAccessor(options.Username, options.Password, options.Domain);
    });
#endif

    builder.Services.AddScoped<ISystemService, SystemService>();
    builder.Services.AddScoped<IDatabaseHealthRepository, DatabaseHealthRepository>();
    builder.Services.AddScoped<IAlbumService, AlbumService>();
    builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
    builder.Services.AddScoped<ITrackService, TrackService>();
    builder.Services.AddScoped<ITracksRepository, TracksRepository>();
    builder.Services.AddScoped<IPlaylistsService, PlaylistsService>();
    builder.Services.AddScoped<IPlaylistsRepository, PlaylistsRepository>();
    builder.Services.AddScoped<ISearchService, SearchService>();
    builder.Services.AddScoped<ISearchRepository, SearchRepository>();
    builder.Services.AddScoped<IHistoryRepository, HistoryRepository>();
    builder.Services.AddScoped<IHistoryService, HistoryService>();
    builder.Services.AddScoped<IGenreRepository, GenreRepository>();
    builder.Services.AddScoped<IGenreService, GenreService>();

    // Use DbContext pooling to reduce allocations and improve throughput under load
    builder.Services.AddDbContextPool<RecordsDbContext>(options =>
    {
        options.UseMySql(connectionStringBuilder.ConnectionString,
            ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString));
    });
    builder.Services.AddAutoMapper(cfg =>
    {
        cfg.LicenseKey = builder.Configuration["AutoMapper:LicenseKey"];
    }, typeof(AlbumProfile));

    // Configure Identity services
    builder.ConfigureBSEIdentity();

    builder.Services.AddEndpointsApiExplorer();

    var apiName = builder.Configuration["Api:Name"] ?? builder.Environment.ApplicationName;
    builder.Services.AddSwaggerGen(options =>
    {
        var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFilename);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);

        // Include Application project XML comments
        var appXmlFile = "BSEtunes.Contracts.xml";
        var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
        if (File.Exists(appXmlPath))
            options.IncludeXmlComments(appXmlPath);

        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = apiName,
            Version = "v1"
        });

        // Configure Swagger to show enums as strings
        options.UseInlineDefinitionsForEnums();

        options.MapType<AlbumSortOption>(() => new OpenApiSchema
        {
            Type = "string",
            Enum = Enum.GetNames<AlbumSortOption>()
                .Select(name => (IOpenApiAny)new OpenApiString(name))
                .ToList()
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Enter 'Bearer {token}'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme, Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
        });

    });

    builder.Services.AddControllers();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    //if (app.Environment.IsDevelopment())
    {
        // Serve static files from wwwroot in development so the external JS is available.
        app.UseStaticFiles();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", apiName);
            options.InjectJavascript("/swagger-ui/custom.js");
            options.EnablePersistAuthorization();
        });
    }

    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);
        };
    });

    //app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();
    // Map Identity API endpoints
    app.MapBSEIdentityApi();
    app.MapControllers();

    await app.RunAsync();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}