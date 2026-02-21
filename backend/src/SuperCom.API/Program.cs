using FluentValidation;
using Microsoft.EntityFrameworkCore;
using SuperCom.API.Middleware;
using SuperCom.Core.Interfaces;
using SuperCom.Core.Validators;
using SuperCom.Infrastructure.Data;
using SuperCom.Infrastructure.Services;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Database
builder.Services.AddDbContext<SuperComDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Services
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ITagService, TagService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateTaskItemValidator>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Global exception handler
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

app.UseCors("AllowReactApp");

app.MapControllers();

// Auto-migrate database on startup in development
if (app.Environment.IsDevelopment())
{
    try
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<SuperComDbContext>();
        if (dbContext.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory")
        {
            dbContext.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        logger.LogWarning(ex, "Could not apply migrations. Is the database running?");
    }
}

app.Run();

// Make the implicit Program class public so test projects can access it
public partial class Program { }
