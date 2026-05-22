using TouristApp.Protos.Tours;
using ToursProto = TouristApp.Protos.Tours.Tours;
using GatewayApi.Grpc;

var builder = WebApplication.CreateBuilder(args);

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var grpcUrl = builder.Configuration["Grpc:TourServiceUrl"] ?? "http://tour-service:81";
builder.Services.AddGrpc().AddJsonTranscoding();
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
app.UseCors("gateway-cors");
app.MapGrpcService<ToursGatewayService>();
app.MapReverseProxy();
app.MapFallback(() => Results.NotFound("Gateway: unknown service"));

app.Run();
