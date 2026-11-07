// ============================================================
// LabBridge — KERNEL (Platform kernel (shared)) — DbContext
// Entities owned by this module: 1.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.Infrastructure.Configurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Common.Idempotency;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; // namespace of OrganizationConfiguration (not provided: adjust)
using Microsoft.EntityFrameworkCore;

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class PlatformKernelDbContext : DbContext
{
    //private static readonly string[] ConfigurationNamespaces =
    //[
    //    "LabBridge.EntityConfigurations.Common",
    //    "LabBridge.EntityConfigurations.OrganizationManagement",
    //];

    // Derived from the types, so a namespace move cannot silently break the filter again.
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(IdempotencyRecordConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace
    ];

    public PlatformKernelDbContext(DbContextOptions<PlatformKernelDbContext> options)
        : base(options) { }

    // ---- Platform kernel (shared) (1) ----
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext"); // needed if the Organization configuration uses citext

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(IdempotencyRecordConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
