using BSEtunes.Application.Mapping;
using BSEtunes.Application.Services;
using BSEtunes.Infrastructure.Data;
using BSEtunes.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionStringBuilder = new MySqlConnectionStringBuilder
{
    Server = builder.Configuration["mysql:server"],
    Database = builder.Configuration["mysql:database"],
    UserID = builder.Configuration["mysql:userid"],
    Password = builder.Configuration["mysql:password"]
};

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
