
using LabBridge.Api.ExceptionHandlers;
using LabBridge.Api.Filters;
using LabBridge.Api.Idempotency;
using LabBridge.Api.Middlewares;
using LabBridge.Api.Security;
using LabBridge.Application;
using LabBridge.Application.Common.Organizations;
using LabBridge.Infrastructure;
using Scalar.AspNetCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddApplication();

builder.Services.AddInfrastructure(builder.Configuration);

// Organization context.
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<ICurrentOrganization, CurrentOrganization>();

// Idempotency.
builder.Services
    .AddOptions<IdempotencyOptions>()
    .BindConfiguration(IdempotencyOptions.SectionName)
    .Validate(
        x => x.Retention > TimeSpan.Zero,
        "Idempotency retention must be greater than zero.")
    .Validate(
        x => x.CleanupInterval > TimeSpan.Zero,
        "Idempotency cleanup interval must be greater than zero.")
    .Validate(
        x => x.CleanupBatchSize is >= 1 and <= 10_000,
        "Idempotency cleanup batch size must be between 1 and 10000.")
    .Validate(
        x => x.MaxRequestBodyBytes > 0,
        "Maximum request body size must be positive.")
    .Validate(
        x => x.MaxResponseBodyBytes > 0,
        "Maximum response body size must be positive.")
    .Validate(
        x => !string.IsNullOrWhiteSpace(x.HeaderName),
        "The idempotency header name is required.")
    .ValidateOnStart();

builder.Services.AddHostedService<IdempotencyCleanupService>();

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

// Must run after routing, authentication, and authorization.
// This middleware reads the selected endpoint's IdempotentAttribute.
app.UseMiddleware<IdempotencyMiddleware>();

app.MapControllers();

app.Run();
