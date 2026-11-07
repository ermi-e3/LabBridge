// ============================================================
// LabBridge — IAM (Identity and Access) — DbContext
// Entities owned by this module: 8.
// Applies every IEntityTypeConfiguration<T> of this module found in
// the LabBridge.EntityConfigurations assembly, plus those of the modules it references.
// ============================================================

using LabBridge.Domain.Features.IdentityAccess.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.IdentityAccess;
using Microsoft.EntityFrameworkCore;
using LabBridge.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;


namespace LabBridge.Infrastructure.Persistence.DbContexts;

//public class IdentityAccessDbContext : DbContext
//{
//    private static readonly string[] ConfigurationNamespaces =
//    [
//        "LabBridge.EntityConfigurations.IdentityAccess",
//        "LabBridge.EntityConfigurations.OrganizationManagement",
//    ];

//    public IdentityAccessDbContext(DbContextOptions<IdentityAccessDbContext> options)
//        : base(options) { }

//    // ---- Identity and Access (8) ----
//    public DbSet<ExternalIdentity> ExternalIdentities => Set<ExternalIdentity>();
//    public DbSet<LocalCredential> LocalCredentials => Set<LocalCredential>();
//    public DbSet<Permission> Permissions => Set<Permission>();
//    public DbSet<Role> Roles => Set<Role>();
//    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
//    public DbSet<User> Users => Set<User>();
//    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();
//    public DbSet<UserRoleAssignment> UserRoleAssignments => Set<UserRoleAssignment>();

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
//        //   Organization and Tenant Management (OrganizationManagement)
//        modelBuilder.ApplyConfigurationsFromAssembly(
//            typeof(UserConfiguration).Assembly,
//            type => ConfigurationNamespaces.Contains(type.Namespace)
//        );
//    }
//}


// ============================================================
// LabBridge — IAM (Identity and Access) — DbContext
// ASP.NET Core Identity tables (users, roles, user_role_assignments, external_identities, user_claims, role_claims,
// user_tokens) plus the module's own tables (permissions, role_permissions, user_invitations).
// Applies every IEntityTypeConfiguration<T> of this module found in the Infrastructure assembly, plus those of the
// modules it references.
// ============================================================



// CHANGE: derives from IdentityDbContext (it was a plain DbContext). The type arguments are explicit because the
// user-role join is custom. Built-in claim/login/token/role-claim types are used otherwise. The key type is Guid
// throughout, matching Entity.Id in the Domain.
public class IdentityAccessDbContext
    : IdentityDbContext<
        ApplicationUser,
        ApplicationRole,
        Guid,
        IdentityUserClaim<Guid>,
        ApplicationUserRole,
        IdentityUserLogin<Guid>,
        IdentityRoleClaim<Guid>,
        IdentityUserToken<Guid>
    >
{
    // CHANGE (critical): the previous filter strings ("LabBridge.EntityConfigurations.*") matched none of the actual
    // configuration namespaces, so NO configuration was applied. The module's namespace is now derived from a type,
    // so it cannot drift.
    // ASSUMPTION: OrganizationManagement configurations live in the namespace below (not among the supplied files).
    private static readonly string?[] ConfigurationNamespaces =
    [
        typeof(ApplicationUserConfiguration).Namespace,
        "LabBridge.Infrastructure.Persistence.Configurations.OrganizationManagement",
    ];

    public IdentityAccessDbContext(DbContextOptions<IdentityAccessDbContext> options)
        : base(options) { }

    // Users, Roles, UserRoles, UserClaims, UserLogins, UserTokens and RoleClaims are inherited from IdentityDbContext.
    // ---- Module-specific tables ----
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserInvitation> UserInvitations => Set<UserInvitation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity's default model first, so our configurations override its defaults (names, lengths, delete behavior).
        base.OnModelCreating(modelBuilder);

        // Configurations of referenced modules are applied too, because entities here have foreign keys to them:
        //   Organization and Tenant Management (OrganizationManagement)
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationUserConfiguration).Assembly,
            type => ConfigurationNamespaces.Contains(type.Namespace)
        );
    }

    // SECURITY: UserManager.AddToRoleAsync creates an ApplicationUserRole through the parameterless constructor, with
    // no AssignedAt/AssignedBy/OrganizationId. That path skips the Domain rules (scope, self-escalation,
    // compatibility). Rows that were not created by ApplicationUser.AssignRole are rejected here.
    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureRoleAssignmentsCreatedThroughAggregate();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default
    )
    {
        EnsureRoleAssignmentsCreatedThroughAggregate();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void EnsureRoleAssignmentsCreatedThroughAggregate()
    {
        var bypassed = ChangeTracker
            .Entries<ApplicationUserRole>()
            .Any(e => e.State == EntityState.Added && e.Entity.AssignedAt == default);
        if (bypassed)
            throw new InvalidOperationException(
                "Role assignments must be created with ApplicationUser.AssignRole, not UserManager.AddToRoleAsync."
            );
    }
}