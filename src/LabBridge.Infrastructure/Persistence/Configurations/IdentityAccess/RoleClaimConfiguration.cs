using LabBridge.Infrastructure.Identity;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Infrastructure.Persistence.Configurations.IdentityAccess;

/// <summary>
/// Standard Identity role claims. Not used by the permission model, which is the Permission/RolePermission catalogue.
/// The table exists because Identity's model requires it.
/// </summary>
public sealed class RoleClaimConfiguration : IEntityTypeConfiguration<IdentityRoleClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityRoleClaim<Guid>> builder)
    {
        builder.ToTable("role_claims");

        builder
            .HasOne<ApplicationRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.UseSnakeCaseColumns();
    }
}