// ============================================================
// LabBridge — AUD (Audit) — DbContext
// Entities owned by this module: 5.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Common.Exceptions;
using LabBridge.Domain.Features.Audit.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Audit;
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement; // namespace of OrganizationConfiguration (not provided: adjust)
using Microsoft.EntityFrameworkCore;

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class AuditDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
      typeof(AuditEventConfiguration).Namespace,
        typeof(OrganizationConfiguration).Namespace,
    ];

    public AuditDbContext(DbContextOptions<AuditDbContext> options)
        : base(options) { }

    // ---- Audit (5) ----
    public DbSet<AuditActionDefinition> AuditActionDefinitions => Set<AuditActionDefinition>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();
    public DbSet<AuditIntegrityCheck> AuditIntegrityChecks => Set<AuditIntegrityCheck>();
    public DbSet<AuditSeal> AuditSeals => Set<AuditSeal>();
    public DbSet<AuditSpoolItem> AuditSpoolItems => Set<AuditSpoolItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasPostgresExtension("citext"); // only if the Organization configuration uses citext

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(AuditEventConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }

    // Defense in depth: audit rows are append-only; only the one-time sealing columns of an event may change.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnforceAppendOnly();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnforceAppendOnly();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnforceAppendOnly()
    {
        foreach (var entry in ChangeTracker.Entries<AuditEvent>())
        {
            if (entry.State == EntityState.Deleted)
                throw new BusinessRuleException("AUDIT_EVENT_IMMUTABLE", "Audit event immutable", "Audit events cannot be deleted.");
            if (entry.State == EntityState.Modified
                && entry.Properties.Any(p => p.IsModified
                    && p.Metadata.Name is not (nameof(AuditEvent.StreamSequence) or nameof(AuditEvent.SealId))))
                throw new BusinessRuleException("AUDIT_EVENT_IMMUTABLE", "Audit event immutable", "Only the sealing columns of an audit event may change.");
        }
        foreach (var entry in ChangeTracker.Entries<AuditSeal>())
            if (entry.State is EntityState.Modified or EntityState.Deleted)
                throw new BusinessRuleException("AUDIT_SEAL_IMMUTABLE", "Audit seal immutable", "Audit seals cannot be changed or deleted.");
    }
}

