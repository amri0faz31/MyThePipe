using VetApi.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddSingleton<VetRepository>();

var app = builder.Build();

// Run migrations automatically
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING");
if (!string.IsNullOrEmpty(connectionString))
{
    DatabaseMigrator.Migrate(connectionString);
}

// Add CORS policy
var allowedOrigins = builder.Configuration["AllowedOrigins"] ?? "http://localhost:5173";
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            policy.WithOrigins(allowedOrigins) // Now configurable!
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});


app.UseHttpsRedirection();
app.UseRouting();

// Enable CORS
app.UseCors("AllowReactApp");

app.UseAuthorization();
app.MapControllers();

app.Run();
