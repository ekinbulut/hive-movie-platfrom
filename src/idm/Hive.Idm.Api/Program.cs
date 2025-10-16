using System.Reflection;
using System.Text;
using Domain.Events;
using Domain.Extension;
using FastEndpoints;
using Hive.Idm.Api.Extensions;
using Infrastructure.Messaging.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Rebus.Config;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add Entity Framework

builder.Services.AddDbContextInfra(builder.Configuration);


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost", policy =>
    {
        policy.WithOrigins("http://localhost:8000", "http://127.0.0.1:8000","http://192.168.1.112:8000")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var featuresAssembly = Assembly.LoadFrom(Path.Combine(AppContext.BaseDirectory, "Hive.Idm.Api.dll"));

builder.Services.AddMediator(
    typeof(Program).Assembly,
    featuresAssembly
);


// Add JWT configuration
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
var issuer = jwtSettings["Issuer"] ?? "HiveIdm";
var audience = jwtSettings["Audience"] ?? "HiveApi";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddFastEndpoints();

// Add Rebus
var rabbitConn =
    builder.Configuration.GetConnectionString("RabbitMq")
    ?? Environment.GetEnvironmentVariable("RABBITMQ__CONNECTION")
    ?? "amqp://guest:guest@localhost:5672";

var inputQueue = "hive-api.path-changed";

// Configure Rebus with routing for multiple queues
builder.Services.AddMessagingWithRouting(rabbitConn, inputQueue, routing =>
{
    // Route WatchPathChangedEvent to hive-api queue
    routing.Map<WatchPathChangedEvent>("hive-api.path-changed");
    // FileFoundEvent uses default queue (hive-watcher)
}, workers: 0);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

}
app.UseCors("AllowLocalhost");
app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

app.Run();