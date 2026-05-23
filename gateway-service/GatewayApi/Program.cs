using TouristApp.Protos.Tours;
using ToursProto = TouristApp.Protos.Tours.Tours;
using GatewayApi.Grpc;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var grpcUrl = builder.Configuration["Grpc:TourServiceUrl"] ?? "http://tour-service:81";
builder.Services.AddGrpc().AddJsonTranscoding();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Gateway API", Version = "v1" });
});
builder.Services.AddReverseProxy().LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("gateway-cors", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});
builder.Services.AddSingleton(sp =>
{
    var channel = Grpc.Net.Client.GrpcChannel.ForAddress(grpcUrl);
    return new ToursProto.ToursClient(channel);
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Gateway API v1");
});

app.UseCors("gateway-cors");

app.MapGet("/tours", async (
    long authorId,
    int page,
    int pageSize,
    ToursProto.ToursClient toursClient) =>
{
    var response = await toursClient.GetByAuthorAsync(new GetByAuthorRequest
    {
        AuthorId = authorId,
        Page = page,
        PageSize = pageSize
    });

    return Results.Ok(new
    {
        tours = response.Tours,
        total = response.Total
    });
})
.WithName("GetToursByAuthor")
.WithSummary("Get tours by author")
.WithDescription("Gateway REST endpoint that forwards to gRPC GetByAuthor.");

app.MapGet("/tours/active", async (
    int page,
    int pageSize,
    ToursProto.ToursClient toursClient) =>
{
    var response = await toursClient.GetActiveAsync(new GetActiveRequest
    {
        Page = page,
        PageSize = pageSize
    });

    return Results.Ok(new
    {
        tours = response.Tours,
        total = response.Total
    });
})
.WithName("GetActiveTours")
.WithSummary("Get active tours")
.WithDescription("Gateway REST endpoint that forwards to gRPC GetActive.");

app.MapGrpcService<ToursGatewayService>();
app.MapReverseProxy();
app.MapFallback(() => Results.NotFound("Gateway: unknown service"));

app.Run();
