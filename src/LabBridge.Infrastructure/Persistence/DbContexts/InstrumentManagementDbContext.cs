// ============================================================
// LabBridge — CON (Instrument and Connector Management) — DbContext
// Entities owned by this module: 12.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; // namespace of OrganizationConfiguration (not provided: adjust)

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class InstrumentManagementDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(InstrumentConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
    ];

    public InstrumentManagementDbContext(DbContextOptions<InstrumentManagementDbContext> options)
        : base(options) { }

    // ---- Instrument and Connector Management (12) ----
    public DbSet<AdapterType> AdapterTypes => Set<AdapterType>();
    public DbSet<Connector> Connectors => Set<Connector>();
    public DbSet<ConnectorConfigRevision> ConnectorConfigRevisions =>
        Set<ConnectorConfigRevision>();
    public DbSet<ConnectorHealthState> ConnectorHealthStates => Set<ConnectorHealthState>();
    public DbSet<ConnectorSecretRef> ConnectorSecretRefs => Set<ConnectorSecretRef>();
    public DbSet<ConnectorTestResult> ConnectorTestResults => Set<ConnectorTestResult>();
    public DbSet<ExternalSystem> ExternalSystems => Set<ExternalSystem>();
    public DbSet<Instrument> Instruments => Set<Instrument>();
    public DbSet<InstrumentVerification> InstrumentVerifications => Set<InstrumentVerification>();
    public DbSet<InterfaceProfile> InterfaceProfiles => Set<InterfaceProfile>();
    public DbSet<InterfaceProfileVersion> InterfaceProfileVersions =>
        Set<InterfaceProfileVersion>();
    public DbSet<Route> Routes => Set<Route>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");   // this module owns the citext columns

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(InstrumentConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
