// ============================================================
// LabBridge — SUB (Subscription Management) — DbContext
// Entities owned by this module: 6.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.SubscriptionManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.SubscriptionManagement;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement;

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class SubscriptionManagementDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
     [
         typeof(SubscriptionConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
     ];

    public SubscriptionManagementDbContext(
        DbContextOptions<SubscriptionManagementDbContext> options
    )
        : base(options) { }

    // ---- Subscription Management (6) ----
    public DbSet<LimitDefinition> LimitDefinitions => Set<LimitDefinition>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<PlanLimit> PlanLimits => Set<PlanLimit>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<SubscriptionHistory> SubscriptionHistories => Set<SubscriptionHistory>();
    public DbSet<UsageCounter> UsageCounters => Set<UsageCounter>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Needed only while Organization.Code is a citext column.
        modelBuilder.HasPostgresExtension("citext");

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(SubscriptionConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
