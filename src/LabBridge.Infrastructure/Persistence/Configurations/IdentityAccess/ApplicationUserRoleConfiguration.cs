using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Identity.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Infrastructure.Persistence.Configurations.IdentityAccess;


/// <summary>
/// Role assignment. The composite key (UserId, RoleId) comes from IdentityUserRole{Guid}; the user-side FK is
/// configured by ApplicationUserConfiguration.
/// </summary>
public sealed class ApplicationUserRoleConfiguration : IEntityTypeConfiguration<ApplicationUserRole>
{
    public void Configure(EntityTypeBuilder<ApplicationUserRole> builder)
    {
        // Table name kept from the previous model.
        builder.ToTable("user_role_assignments", "identity");

        builder.Property(x => x.AssignedAt).IsRequired();

        // CHANGE: Identity's default role FK cascades on delete. This module never hard-deletes (BR-IAM-010), so it is RESTRICT.
        builder
            .HasOne<ApplicationRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.RoleId }); // last-administrator counting (BR-IAM-006)

        builder.UseSnakeCaseColumns();
    }
}