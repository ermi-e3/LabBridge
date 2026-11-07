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
/// Standard Identity user tokens (authenticator keys, recovery codes and the like).
/// SECURITY: Value is stored as plain text by default. If authenticator keys or recovery codes are used, enable
/// Identity's personal-data protection (IdentityOptions.Stores.ProtectPersonalData with an ILookupProtector).
/// </summary>
public sealed class UserTokenConfiguration : IEntityTypeConfiguration<IdentityUserToken<Guid>>
{
    public void Configure(EntityTypeBuilder<IdentityUserToken<Guid>> builder)
    {
        builder.ToTable("user_tokens", "identity");

        builder
            .HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.UseSnakeCaseColumns();
    }
}