using ECommerce_Project;
using static ECommerce_Project.ClientClass;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using ECommerce_Project.Models;
using Microsoft.Extensions.FileProviders;

// Main application startup file - configures all services and middleware

var builder = WebApplication.CreateBuilder(args);

// ===== SERVICE CONFIGURATION =====

// Add MVC controllers to handle API requests
builder.Services.AddControllers();

// Configure CORS (Cross-Origin Resource Sharing) to allow frontend to call backend
builder.Services.AddCors(options =>
{
    options.AddPolicy("EnableCORS", builder =>
    {
        builder.AllowAnyOrigin()    // Allow requests from any domain (for development)
        .AllowAnyHeader()           // Allow any HTTP headers
        .AllowAnyMethod();          // Allow any HTTP methods (GET, POST, etc.)
    });
});

// Configure JWT Authentication
builder.Services.AddAuthentication(opt => {
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme; // Use JWT for authentication
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;    // Use JWT for challenges
})
    .AddJwtBearer(options =>
    {
        // Configure how JWT tokens are validated
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,      // Check who issued the token
            ValidateAudience = true,    // Check who the token is for
            ValidateLifetime = true,    // Check if token is expired
            ValidateIssuerSigningKey = true, // Verify token signature
            ValidIssuer = "https://localhost:7077",     // Who can issue tokens
            ValidAudience = "https://localhost:7077",   // Who can use tokens
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("superSecretKey@345superSecretKey@345superSecretKey@345")) // Secret key to verify tokens
        };
    });

// Add API documentation services
builder.Services.AddEndpointsApiExplorer(); // Reads metadata from API methods
builder.Services.AddSwaggerGen();           // Generates Swagger documentation

// Add HTTP client for external API calls
builder.Services.AddHttpClient();

// Register custom services (for external API integration)
builder.Services.AddScoped<IMyinter, ClientClass>();
builder.Services.AddHttpClient<IMyinter, ClientClass>(c =>
{
    c.BaseAddress = new Uri("https://api.restful-api.dev/"); // External API base URL
});

// ===== BUILD APPLICATION =====
var app = builder.Build();

// ===== MIDDLEWARE PIPELINE =====

// Enable Swagger documentation in development mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();     // Generate Swagger JSON
    app.UseSwaggerUI();   // Serve Swagger UI at /swagger
}

// Redirect HTTP to HTTPS for security
app.UseHttpsRedirection();

// Enable CORS to allow frontend requests
app.UseCors("EnableCORS");

// Serve static files (images) from Images folder
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Images")),
    RequestPath = "/Images"
});

// Enable JWT authentication (must come before authorization)
app.UseAuthentication();

// Enable authorization (checks [Authorize] attributes)
app.UseAuthorization();

// Map controller routes
app.MapControllers();

// Start the application
app.Run();
