using DotnetGRPC.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(8080);
    options.ListenAnyIP(8585, listenOptions =>
    {
        listenOptions.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2;
    });
});

builder.Services.AddGrpcSwagger();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new OpenApiInfo { Title = "gRPC transcoding", Version = "v1" });

});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
});

if (app.Environment.IsDevelopment())
{
    var config = new ConfigurationBuilder()
                        .AddUserSecrets<Program>()
                        .Build();
    DotnetGRPC.GlobalVariables.Blob.Key = config["SPACE_KEY"];
    DotnetGRPC.GlobalVariables.Blob.Secret = config["SPACE_SECRET"];
}
else
{
    DotnetGRPC.GlobalVariables.Blob.Key = builder.Configuration["SPACES_KEY"];
    DotnetGRPC.GlobalVariables.Blob.Secret = builder.Configuration["SPACES_SECRET"];
}

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FileTransferService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGrpcReflectionService();

app.Run();
