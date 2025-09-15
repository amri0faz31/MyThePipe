using VetApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSingleton<VetRepository>();

// Add CORS policy
// Smart CORS configuration
var isProduction = builder.Environment.IsProduction();
var allowedOriginsConfig = builder.Configuration["AllowedOrigins"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApp",
        policy =>
        {
            if (isProduction)
            {
                // Production: Allow same origin (frontend served by backend) + any configured external origins
                if (!string.IsNullOrEmpty(allowedOriginsConfig))
                {
                    var originsList = allowedOriginsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    policy.WithOrigins(originsList);
                }
                // In production, same-origin requests are automatically allowed
                // even without explicit CORS configuration
                policy.AllowAnyOrigin(); // Or be more specific if needed
            }
            else
            {
                // Development/Testing: Allow localhost for React dev server
                policy.WithOrigins("http://localhost:5173", "https://localhost:5173");
            }
            
            policy.AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

// Run migrations automatically ONLY in Production/Staging
// Skip migrations in Test/Development environments
if (app.Environment.IsProduction() || app.Environment.IsStaging())
{
    var connectionString = app.Configuration.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        try
        {
            DatabaseMigrator.Migrate(connectionString);
            Console.WriteLine("✅ Database migrations completed successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Database migration failed: {ex.Message}");
            // Don't crash the app - maybe it's already migrated
        }
    }
    else
    {
        Console.WriteLine("⚠️ No connection string found for migrations");
    }
}

// Use HTTPS only in Production
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

// ⚠️ REMOVED HTTPS redirection for development
// app.UseHttpsRedirection();

app.UseRouting();

// Enable CORS middleware
app.UseCors("AllowReactApp");

app.UseAuthorization();
app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();