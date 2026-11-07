// ============================================================
// LabBridge — DLV (Delivery) — DbContext
// Entities owned by this module: 7.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.DeliveryManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.DeliveryManagement;
using LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;   // namespace of ConnectorConfiguration (not provided: adjust)
using LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;       // namespace of InboundMessageConfiguration (not provided: adjust)
using LabBridge.Infrastructure.Persistence.Configurations.OrderResultManagement;  // namespace of DeliveryRequestConfiguration (not provided: adjust)
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; // namespace of OrganizationConfiguration (not provided: adjust)

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class DeliveryManagementDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
     [
         typeof(DeliveryItemConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
        typeof(ConnectorConfiguration).Namespace,
        typeof(InboundMessageConfiguration).Namespace,
        typeof(DeliveryRequestConfiguration).Namespace,
    ];

    public DeliveryManagementDbContext(DbContextOptions<DeliveryManagementDbContext> options)
        : base(options) { }

    // ---- Delivery (7) ----
    public DbSet<AcknowledgmentRecord> AcknowledgmentRecords => Set<AcknowledgmentRecord>();
    public DbSet<DeliveryAttempt> DeliveryAttempts => Set<DeliveryAttempt>();
    public DbSet<DeliveryItem> DeliveryItems => Set<DeliveryItem>();
    public DbSet<ReconciliationCase> ReconciliationCases => Set<ReconciliationCase>();
    public DbSet<RouteDeliveryPolicy> RouteDeliveryPolicies => Set<RouteDeliveryPolicy>();
    public DbSet<SimulatorBehavior> SimulatorBehaviors => Set<SimulatorBehavior>();
    public DbSet<SimulatorSinkMessage> SimulatorSinkMessages => Set<SimulatorSinkMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");   // only if referenced configurations use citext

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement), Instrument and Connector Management (InstrumentManagement), Message Ingestion (MessageIngestion), Order and Result Workflow (OrderResultManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(DeliveryItemConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
