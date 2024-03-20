

using System.Reflection.Metadata;

var builder = WebApplication.CreateBuilder(args);




// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


Console.WriteLine("Access key: " + builder.Configuration["SPACES_KEY"]);
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    var config = new ConfigurationBuilder()
                        .AddUserSecrets<Program>()
                        .Build();

    DotnetAPI.Constants.Keys.SpacesKey = config["SPACES_KEY"];
    DotnetAPI.Constants.Keys.SpacesSecret = config["SPACES_SECRET"];
}
else
{
    DotnetAPI.Constants.Keys.SpacesKey = builder.Configuration["SPACES_KEY"];
    DotnetAPI.Constants.Keys.SpacesSecret = builder.Configuration["SPACES_SECRET"];
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
