using LabBridge.Infrastructure.Identity;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Infrastructure.Persistence.Configurations.IdentityAccess;


/// <summary>Standard Identity user claims. Only table/column naming and delete behavior are aligned with the project conventions.</summary>
public sealed class UserClaimConfiguration : IEntityTypeConfiguration<IdentityUserClaim<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserClaim<Guid>> builder)
    {
        builder.ToTable("user_claims", "identity");

        // IMPROVEMENT: snake_case table names and RESTRICT instead of Identity's CASCADE (users are never hard-deleted).
        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.UseSnakeCaseColumns();
    }
}