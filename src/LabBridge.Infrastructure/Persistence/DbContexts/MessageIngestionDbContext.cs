// ============================================================
// LabBridge — ING (Message Ingestion) — DbContext
// Entities owned by this module: 6.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.Infrastructure.Persistence.Configurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;   // namespace of ConnectorConfiguration (not provided: adjust)
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; // namespace of OrganizationConfiguration (not provided: adjust)

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class MessageIngestionDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(InboundMessageConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
        typeof(ConnectorConfiguration).Namespace,
    ];

    public MessageIngestionDbContext(DbContextOptions<MessageIngestionDbContext> options)
        : base(options) { }

    // ---- Message Ingestion (6) ----
    public DbSet<DeduplicationKey> DeduplicationKeys => Set<DeduplicationKey>();
    public DbSet<InboundMessage> InboundMessages => Set<InboundMessage>();
    public DbSet<InboundMessageStatusHistory> InboundMessageStatusHistories =>
        Set<InboundMessageStatusHistory>();
    public DbSet<InboundProcessingHandoff> InboundProcessingHandoffs =>
        Set<InboundProcessingHandoff>();
    public DbSet<InboundRefusal> InboundRefusals => Set<InboundRefusal>();
    public DbSet<RawPayload> RawPayloads => Set<RawPayload>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");                 // referenced Instrument tables use citext
        modelBuilder.HasSequence<long>("inbound_message_receipt_seq"); // so the migration creates it

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement), Instrument and Connector Management (InstrumentManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InboundMessageConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
