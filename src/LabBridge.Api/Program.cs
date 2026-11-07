
using LabBridge.Domain.Features.Audit.Entities;
using LabBridge.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();
//Console.WriteLine("========== DIAGNOSTIC STARTED ==========");

//using (var scope = app.Services.CreateScope())
//{
//    var dbContext = scope.ServiceProvider
//        .GetRequiredService<LabBridgeDbContext>();

//    var entityType = dbContext.Model.FindEntityType(typeof(AuditSeal));

//    Console.WriteLine($"Entity found: {entityType is not null}");
//    Console.WriteLine($"Table: {entityType?.GetTableName() ?? "<null>"}");
//    Console.WriteLine($"Schema: {entityType?.GetSchema() ?? "<null>"}");
//}

//Console.WriteLine("========== DIAGNOSTIC FINISHED ==========");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
