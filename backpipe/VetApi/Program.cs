using VetApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSingleton<VetRepository>();

// Add CORS policy
var allowedOrigins = builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyHeader()
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
        DatabaseMigrator.Migrate(connectionString);
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