
using LabBridge.Api.ExceptionHandlers;
using LabBridge.Api.Filters;
using LabBridge.Api.Middlewares;
using LabBridge.Application;
using LabBridge.Infrastructure;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

//builder.Services.AddControllers();
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AuditLogFilter>();
}).AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(
        new JsonStringEnumConverter()); // this will help to convert the enums to string and vic versa
});


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Centralized exception handling.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Register ProblemDetails support.
builder.Services.AddProblemDetails();

var app = builder.Build();
// Must be early in the middleware pipeline so it can
// handle exceptions thrown by subsequent middleware and endpoints.
app.UseExceptionHandler();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
