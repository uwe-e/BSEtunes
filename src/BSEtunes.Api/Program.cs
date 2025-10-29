using BSEtunes.Application.Mapping;
using BSEtunes.Application.Services;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MySqlConnector;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services.AddScoped<IAlbumService, AlbumService>();
builder.Services.AddScoped<IAlbumRepository, AlbumRepository>();
builder.Services.AddDbContext<RecordsDbContext>(options =>
{
    options.UseMySql(connectionStringBuilder.ConnectionString,
        ServerVersion.AutoDetect(connectionStringBuilder.ConnectionString));
});
builder.Services.AddAutoMapper(cfg => { }, typeof(AlbumProfile));

builder.Services.AddEndpointsApiExplorer();

var apiName = builder.Configuration["Api:Name"] ?? builder.Environment.ApplicationName;
builder.Services.AddSwaggerGen(options =>
{
    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
    
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = apiName,
        Version = "v1"
    });
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
