using TouristApp.API.Middleware;
using TouristApp.API.Startup;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.ConfigureSwagger(builder.Configuration);
const string corsPolicy = "_corsPolicy";
builder.Services.ConfigureCors(corsPolicy);

builder.Services.RegisterModules();

var app = builder.Build();

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
app.UseHttpsRedirection();

app.MapControllers();

app.Run();

// Required for automated tests
namespace TouristApp.API
{
    public partial class Program { }
}
