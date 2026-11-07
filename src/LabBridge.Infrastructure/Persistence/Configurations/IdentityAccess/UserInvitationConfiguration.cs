using LabBridge.Domain.Features.IdentityAccess.Entities;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.IdentityAccess;

/// <summary>Secret-derived: only the token hash is stored. Single use is enforced by a conditional update (UsedAt IS NULL) at the persistence boundary.</summary>
public sealed class UserInvitationConfiguration : IEntityTypeConfiguration<UserInvitation>
{
    public void Configure(EntityTypeBuilder<UserInvitation> builder)
    {
        builder.ToTable("user_invitations");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.UserId).IsRequired();
        builder.Property(x => x.TokenHash).HasColumnType("bytea").IsRequired();
        builder.Property(x => x.ExpiresAt).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(x => x.OrganizationId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.TokenHash).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.ExpiresAt); // purge job

        builder.UseSnakeCaseColumns();
    }
}
