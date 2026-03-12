using Dino_runner.Data;
using Microsoft.EntityFrameworkCore;

namespace Dino_runner;

/// <summary>
/// Application entry point.
/// Configures services, middleware and startup behavior for the Dino Runner project.
/// </summary>
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register MVC controllers so frontend pages can call backend APIs.
        builder.Services.AddControllers();

        // Register Swagger for local API testing in development mode.
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Register SQLite database context.
        // Connection string is read from appsettings.json.
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(builder.Configuration.GetConnectionString("Default")));

        // Allow frontend pages and backend APIs to communicate without CORS blocking.
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // Enable Swagger UI only in development.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        // Ensure the SQLite database and seeded data are created when the app starts.
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
        }

        // Standard middleware pipeline.
        app.UseHttpsRedirection();
        app.UseCors("AllowAll");
        app.UseAuthorization();

        // Serve static frontend files from wwwroot.
        app.UseStaticFiles();

        // Map backend API controllers first.
        // This avoids fallback routing interfering with /api/* requests.
        app.MapControllers();

        // Fallback route for direct browser access to frontend pages.
        app.MapFallbackToFile("index.html");

        app.Run();
    }
}
