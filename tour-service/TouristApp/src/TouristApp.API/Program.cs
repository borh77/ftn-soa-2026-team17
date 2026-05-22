using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using TouristApp.API.Grpc;
using TouristApp.API.Authentification;
using TouristApp.API.Middleware;
using TouristApp.API.Startup;
using TouristApp.Tours.Infrastructure.Database;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddGrpc().AddJsonTranscoding();
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
            context.Database.ExecuteSqlRaw("""
                CREATE TABLE IF NOT EXISTS "Tours"."TourReviews" (
                    "Id" bigint GENERATED ALWAYS AS IDENTITY,
                    "TourId" bigint NOT NULL,
                    "TouristId" bigint NOT NULL,
                    "TouristUsername" character varying(200) NOT NULL,
                    "Rating" integer NOT NULL,
                    "Comment" character varying(2000) NOT NULL,
                    "VisitedAt" timestamp with time zone NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "Images" jsonb NOT NULL,
                    CONSTRAINT "PK_TourReviews" PRIMARY KEY ("Id"),
                    CONSTRAINT "FK_TourReviews_Tours_TourId" FOREIGN KEY ("TourId")
                        REFERENCES "Tours"."Tours" ("Id") ON DELETE CASCADE,
                    CONSTRAINT "CK_TourReviews_Rating" CHECK ("Rating" >= 1 AND "Rating" <= 5)
                );

                CREATE INDEX IF NOT EXISTS "IX_TourReviews_TourId"
                    ON "Tours"."TourReviews" ("TourId");

                ALTER TABLE "Tours"."TourReviews"
                    ADD COLUMN IF NOT EXISTS "TouristUsername" character varying(200) NOT NULL DEFAULT '';

                ALTER TABLE "Tours"."TourReviews"
                    ADD COLUMN IF NOT EXISTS "VisitedAt" timestamp with time zone NOT NULL DEFAULT now();

                ALTER TABLE "Tours"."TourReviews"
                    ADD COLUMN IF NOT EXISTS "CreatedAt" timestamp with time zone NOT NULL DEFAULT now();

                ALTER TABLE "Tours"."TourReviews"
                    ADD COLUMN IF NOT EXISTS "Images" jsonb NOT NULL DEFAULT '[]'::jsonb;

                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1
                        FROM information_schema.columns
                        WHERE table_schema = 'Tours'
                          AND table_name = 'TourReviews'
                          AND column_name = 'ImageUrls'
                    ) THEN
                        ALTER TABLE "Tours"."TourReviews"
                            ALTER COLUMN "ImageUrls" DROP NOT NULL;
                    END IF;
                END $$;
                """);
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
app.MapGrpcService<ToursGrpcService>();

app.Run();

// Required for automated tests
namespace TouristApp.API
{
    public partial class Program { }
}
