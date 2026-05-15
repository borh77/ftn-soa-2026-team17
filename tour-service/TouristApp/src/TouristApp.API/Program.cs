using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TouristApp.API.Authentification;
using TouristApp.API.Middleware;
using TouristApp.API.Startup;
using TouristApp.Tours.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.ConfigureSwagger(builder.Configuration);
const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);

builder.Services.RegisterModules();

builder.Services.AddScoped<JwtService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secret = builder.Configuration["Jwt:Secret"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secret!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("touristPolicy", policy =>
        policy.RequireRole("TOURIST"));

    options.AddPolicy("guidePolicy", policy =>
        policy.RequireRole("GUIDE"));

    options.AddPolicy("adminPolicy", policy =>
        policy.RequireRole("ADMIN"));
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ToursContext>();

    const int maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"Pokušaj inicijalizacije baze {attempt}/{maxRetries}...");
            context.Database.EnsureCreated();
            Console.WriteLine("Baza je uspešno inicijalizovana.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Inicijalizacija baze nije uspela u pokušaju {attempt}: {ex.Message}");

            if (attempt == maxRetries)
            {
                throw;
            }

            Thread.Sleep(delay);
        }
    }
}

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHsts();
}

app.UseRouting();
app.UseCors(corsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Required for automated tests
namespace TouristApp.API
{
    public partial class Program { }
}
