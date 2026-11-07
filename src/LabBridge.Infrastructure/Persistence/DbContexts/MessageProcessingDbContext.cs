// ============================================================
// LabBridge — PRC (Message Processing) — DbContext
// Entities owned by this module: 7.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement; 
using LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;       
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; 

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class MessageProcessingDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(ProcessingRunConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
        typeof(ConnectorConfiguration).Namespace,
        typeof(InboundMessageConfiguration).Namespace,
    ];

    public MessageProcessingDbContext(DbContextOptions<MessageProcessingDbContext> options)
        : base(options) { }

    // ---- Message Processing (7) ----
    public DbSet<NormalizedHandoff> NormalizedHandoffs => Set<NormalizedHandoff>();
    public DbSet<NormalizedMessage> NormalizedMessages => Set<NormalizedMessage>();
    public DbSet<ParserType> ParserTypes => Set<ParserType>();
    public DbSet<ProcessingReview> ProcessingReviews => Set<ProcessingReview>();
    public DbSet<ProcessingRun> ProcessingRuns => Set<ProcessingRun>();
    public DbSet<ProfileParserBinding> ProfileParserBindings => Set<ProfileParserBinding>();
    public DbSet<ValidationFinding> ValidationFindings => Set<ValidationFinding>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");   // referenced Instrument tables use citext

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement), Instrument and Connector Management (InstrumentManagement), Message Ingestion (MessageIngestion)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ProcessingRunConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
