using Microsoft.OpenApi.Models;
using TouristApp.API.Middleware;
using TouristApp.API.Startup;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TouristApp.Blog.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.ConfigureSwagger(builder.Configuration);
const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("touristPolicy", policy =>
        policy.RequireAuthenticatedUser());
});

builder.Services.RegisterModules();

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
    try
    {
        var context = services.GetRequiredService<BlogContext>();

        if (context.Database.EnsureCreated())
        {
            Console.WriteLine("TABELE SU USPEŠNO KREIRANE IZ KODA!");
        }
        else
        {
            Console.WriteLine("Tabele već postoje ili su migracije već odrađene.");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"GREŠKA: {ex.Message}");
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
