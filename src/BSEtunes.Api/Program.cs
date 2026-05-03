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
using Serilog.Events;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
    .MinimumLevel.Override("BSEtunes", LogEventLevel.Debug)
    .Enrich.FromLogContext()
    //.Enrich.WithThreadId()
    //.Enrich.WithMachineName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/bsetunes-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u3}] {SourceContext} - {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

try
{
    Log.Information("Starting BSEtunes API");

    // Add services to the container.
    var connectionStringBuilder = new MySqlConnectionStringBuilder
    {
        Server = builder.Configuration["mysql:server"],
        Database = builder.Configuration["mysql:database"],
        UserID = builder.Configuration["mysql:userid"],
        Password = builder.Configuration["mysql:password"]
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
    builder.Configuration.GetSection("FileShare"));

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

    builder.Services.AddDbContext<RecordsDbContext>(options =>
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
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

        // Include Application project XML comments
        var appXmlFile = "BSEtunes.Contracts.xml";
        options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, appXmlFile));

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
    if (app.Environment.IsDevelopment())
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

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();
    // Map Identity API endpoints
    app.MapBSEIdentityApi();
    app.MapControllers();

    app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}