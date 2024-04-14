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

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddGrpcReflection();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

Console.WriteLine(builder.Configuration.GetConnectionString("DefaultConnection"));
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
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new PostgreSqlStorageOptions
    {
        QueuePollInterval = TimeSpan.FromMilliseconds(50),
    }));

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

app.UseHangfireDashboard();
app.UseHangfireServer();

RecurringJob.AddOrUpdate<NotificationService>(service => service.SendNotifyPendingContribution(), Cron.Daily);

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FileTransferService>();
app.MapGrpcService<NotificationService>();
app.MapGrpcService<DatabaseService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.MapGrpcReflectionService();

app.Run();
