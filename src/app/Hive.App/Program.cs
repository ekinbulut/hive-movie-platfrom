using FastEndpoints;
using Features.Extensions;
using Infrastructure.Database.Extensions;
using Microsoft.AspNetCore.Authentication.Negotiate;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAuthentication(NegotiateDefaults.AuthenticationScheme)
    .AddNegotiate();

builder.Services.AddAuthorization(options =>
{
    // By default, all incoming requests will be authorized according to the default policy.
    // options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddFastApiRoutes();

builder.Services.AddDbContext(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseFastEndpoints(config =>
{
    config.Endpoints.RoutePrefix = "v1/api";
});

app.Run();