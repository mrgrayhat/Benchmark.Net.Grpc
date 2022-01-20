using Benchmark.Net.Grpc.Server.DAL;
using Benchmark.Net.Grpc.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});
//builder.Services.AddLogging();
    //builder.Services.AddHttpLogging((opt) =>
    //{
    //});
builder.Services.AddGrpc();

var app = builder.Build();

DbContext.Seed();

// Configure the HTTP request pipeline.
app.MapGrpcService<ChatServiceGrpc>();
app.MapDefaultControllerRoute();

app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
