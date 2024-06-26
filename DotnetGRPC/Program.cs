using DotnetGRPC.Model.DTO;
using DotnetGRPC.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.PostgreSql;

using Microsoft.Extensions.Azure;
using Azure.Identity;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.PostgreSql;
using Azure.ResourceManager.PostgreSql.FlexibleServers;
using Microsoft.Azure.Management.WebSites;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

var credential = new Microsoft.Rest.TokenCredentials(DotnetGRPC.GlobalVariables.Database.BackupToken);
var webSiteManagementClient = new Microsoft.Azure.Management.WebSites.WebSiteManagementClient(credential)
{
    SubscriptionId = "5f459f53-780f-4ffc-8604-0e47bbbfb746"
};
var appSettings = await webSiteManagementClient.WebApps.ListApplicationSettingsAsync("Comp-1640", "comp1640api");
Console.WriteLine(appSettings.Properties["DefaultConnection"]);
// Retrieve the DefaultConnection setting

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(appSettings.Properties["DefaultConnection"]));

// Add repositories
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<TemplateRepository>();
builder.Services.AddScoped<NotificationRepository>();
builder.Services.AddScoped<FacultyRepository>();
builder.Services.AddScoped<ContributionRepository>();

// Add services
builder.Services.AddScoped<NotificationService>();

// Add email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Add Hangfire services
builder.Services.AddHangfire(x =>
    x.UsePostgreSqlStorage(builder.Configuration.GetConnectionString("HangfireConnection")));

builder.Services.AddHangfireServer();

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

Console.WriteLine($"Backup Token: {DotnetGRPC.GlobalVariables.Database.BackupToken}");

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


}
else
{
    DotnetGRPC.GlobalVariables.Blob.Key = builder.Configuration["SPACES_KEY"];
    DotnetGRPC.GlobalVariables.Blob.Secret = builder.Configuration["SPACES_SECRET"];
}

app.UseHangfireServer();

RecurringJob.AddOrUpdate<NotificationService>(service => service.SendNotifyPendingContribution(), Cron.Daily);

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FileTransferService>();
app.MapGrpcService<NotificationService>();
app.MapGrpcService<DatabaseService>();
app.MapGrpcService<ReportServiceImp>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGrpcReflectionService();

app.Run();
