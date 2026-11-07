// ============================================================
// LabBridge — MON (Monitoring and Alerting) — DbContext
// Entities owned by this module: 6.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.Monitoring.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Monitoring;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Persistence.Configurations.InstrumentManagement;   
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; 

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class MonitoringDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(AlertConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
        typeof(ConnectorConfiguration).Namespace,
    ];

    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options)
        : base(options) { }

    // ---- Monitoring and Alerting (6) ----
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<AlertRuleSetting> AlertRuleSettings => Set<AlertRuleSetting>();
    public DbSet<AlertStateHistory> AlertStateHistories => Set<AlertStateHistory>();
    public DbSet<ConnectorHealthSnapshot> ConnectorHealthSnapshots =>
        Set<ConnectorHealthSnapshot>();
    public DbSet<MetricSample> MetricSamples => Set<MetricSample>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext");   // referenced Organization/Instrument tables use citext

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement), Instrument and Connector Management (InstrumentManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AlertConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
