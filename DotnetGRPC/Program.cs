using DotnetGRPC;
using DotnetGRPC.Services;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using Azure.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();



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
    // builder.Services.AddAzureClients(x => {
    //     x.UseCredential(new DefaultAzureCredential());
    // });
    
    Console.WriteLine($"Backup Token: {DotnetGRPC.GlobalVariables.Database.BackupToken}");

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
