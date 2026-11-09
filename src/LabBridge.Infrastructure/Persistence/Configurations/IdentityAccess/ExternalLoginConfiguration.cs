using LabBridge.Infrastructure.Identity.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace LabBridge.Infrastructure.Persistence.Configurations.IdentityAccess;

/// <summary>
/// External identity (issuer + subject) mapped to Identity's built-in login type. LoginProvider = issuer,
/// ProviderKey = subject. The composite PK (LoginProvider, ProviderKey) makes (issuer, subject) globally unique and
/// prevents duplicate binding under race (BR-IAM-016), replacing the former unique index.
/// </summary>
public sealed class ExternalLoginConfiguration : IEntityTypeConfiguration<IdentityUserLogin<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserLogin<Guid>> builder)
    {
        builder.ToTable("external_identities", "identity");

        // CHANGE: lengths match the previous ExternalIdentity limits (issuer 500, subject 255).
        builder.Property(x => x.LoginProvider).HasMaxLength(500);
        builder.Property(x => x.ProviderKey).HasMaxLength(255);
        builder.Property(x => x.ProviderDisplayName).HasMaxLength(200);

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.UseSnakeCaseColumns();
    }
}