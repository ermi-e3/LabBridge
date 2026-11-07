// ============================================================
// LabBridge — RPT (Reporting) — DbContext
// Entities owned by this module: 3.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.Reporting.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Reporting;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement;

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class ReportingDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(ReportRunConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
    ];

    public ReportingDbContext(DbContextOptions<ReportingDbContext> options)
        : base(options) { }

    // ---- Reporting (3) ----
    public DbSet<DailyFact> DailyFacts => Set<DailyFact>();
    public DbSet<ReportDefinition> ReportDefinitions => Set<ReportDefinition>();
    public DbSet<ReportRun> ReportRuns => Set<ReportRun>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Needed only while Organization.Code is a citext column.
        modelBuilder.HasPostgresExtension("citext");

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ReportRunConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
