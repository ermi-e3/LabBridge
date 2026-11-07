// ============================================================
// LabBridge — WFL (Order and Result Workflow) — DbContext
// Entities owned by this module: 7.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.OrderResultManagement;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;   
using LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;       
using LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;      
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; 
using LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;        

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class OrderResultManagementDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(OrderConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
        typeof(ConnectorConfiguration).Namespace,
        typeof(InboundMessageConfiguration).Namespace,
        typeof(ProcessingRunConfiguration).Namespace,
        typeof(MappingRunConfiguration).Namespace,
    ];

    public OrderResultManagementDbContext(DbContextOptions<OrderResultManagementDbContext> options)
        : base(options) { }

    // ---- Order and Result Workflow (7) ----
    public DbSet<DeliveryRequest> DeliveryRequests => Set<DeliveryRequest>();
    public DbSet<Observation> Observations => Set<Observation>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderLine> OrderLines => Set<OrderLine>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Result> Results => Set<Result>();
    public DbSet<ResultStatusHistory> ResultStatusHistories => Set<ResultStatusHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");   // referenced Organization/Instrument tables use citext

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement), Instrument and Connector Management (InstrumentManagement), Message Ingestion (MessageIngestion), Message Processing (MessageProcessing), Test Code Mapping (TestCodeMapping)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrderConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
