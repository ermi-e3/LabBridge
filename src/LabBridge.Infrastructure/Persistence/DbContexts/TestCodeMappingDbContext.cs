// ============================================================
// LabBridge — MAP (Test Code Mapping) — DbContext
// Entities owned by this module: 9.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;   
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; 

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class TestCodeMappingDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(MappingSetConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
        typeof(InstrumentConfiguration).Namespace,
    ];

    public TestCodeMappingDbContext(DbContextOptions<TestCodeMappingDbContext> options)
        : base(options) { }

    // ---- Test Code Mapping (9) ----
    public DbSet<CanonicalConcept> CanonicalConcepts => Set<CanonicalConcept>();
    public DbSet<MappingEntry> MappingEntries => Set<MappingEntry>();
    public DbSet<MappingGap> MappingGaps => Set<MappingGap>();
    public DbSet<MappingGapSubject> MappingGapSubjects => Set<MappingGapSubject>();
    public DbSet<MappingResolution> MappingResolutions => Set<MappingResolution>();
    public DbSet<MappingRun> MappingRuns => Set<MappingRun>();
    public DbSet<MappingSet> MappingSets => Set<MappingSet>();
    public DbSet<MappingSetVersion> MappingSetVersions => Set<MappingSetVersion>();
    public DbSet<UnitConversionRule> UnitConversionRules => Set<UnitConversionRule>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");   // referenced Organization/Instrument tables use citext

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement), Instrument and Connector Management (InstrumentManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(MappingSetConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
