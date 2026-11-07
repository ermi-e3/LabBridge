// ============================================================
// LabBridge — ORG (Organization and Tenant Management) — DbContext
// Entities owned by this module: 4.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly.
// ============================================================

using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement;
using Microsoft.EntityFrameworkCore;

namespace LabBridge.Infrastructure.Persistence.DbContexts;

public class OrganizationManagementDbContext : DbContext
{
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(OrganizationConfiguration).Namespace,
    ];

    public OrganizationManagementDbContext(
        DbContextOptions<OrganizationManagementDbContext> options
    )
        : base(options) { }

    // ---- Organization and Tenant Management (4) ----
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationSetting> OrganizationSettings => Set<OrganizationSetting>();
    public DbSet<OrganizationStatusHistory> OrganizationStatusHistories =>
        Set<OrganizationStatusHistory>();
    public DbSet<SettingDefinition> SettingDefinitions => Set<SettingDefinition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)      
    {
        base.OnModelCreating(modelBuilder);

        // No citext extension is needed in this module once Organization.Code is a plain varchar (see OrganizationConfiguration).

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(OrganizationConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }
}
