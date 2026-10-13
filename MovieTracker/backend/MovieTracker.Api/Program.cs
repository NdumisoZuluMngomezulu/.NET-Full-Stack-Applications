using Microsoft.EntityFrameworkCore;
using MovieTracker.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=movies.db"));

builder.Services.AddControllers();

var app = builder.Build();

// Create the SQLite file and a few sample movies on first run.
// For a real deployment, switch to EF Core migrations (dotnet ef migrations add Initial).
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    SeedData.Seed(db);
}

// In production the built React app lives in wwwroot and is served from the same origin as the API.
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
