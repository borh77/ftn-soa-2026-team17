using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TouristApp.API.Authentification;
using TouristApp.API.Middleware;
using TouristApp.API.Startup;
using Microsoft.EntityFrameworkCore;
using TouristApp.Blog.Infrastructure.Database;

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

builder.Services.AddSwaggerGen(c =>
{
    
   
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}
        }
    });
});
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<BlogContext>();

    const int maxRetries = 10;
    var delay = TimeSpan.FromSeconds(5);

    for (int attempt = 1; attempt <= maxRetries; attempt++)
    {
        try
        {
            Console.WriteLine($"Pokušaj primene migracija {attempt}/{maxRetries}...");
            context.Database.Migrate();
            Console.WriteLine("Migracije su uspešno primenjene.");
            break;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Migracije nisu uspele u pokušaju {attempt}: {ex.Message}");

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
app.UseAuthentication(); // Prepoznaje ko je korisnik na osnovu tokena
app.UseAuthorization();
app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Required for automated tests
namespace TouristApp.API
{
    public partial class Program { }
}
