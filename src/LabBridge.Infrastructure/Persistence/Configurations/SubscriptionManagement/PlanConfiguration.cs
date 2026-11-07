using LabBridge.Domain.Features.SubscriptionManagement.Entities;
using LabBridge.Domain.Features.SubscriptionManagement.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.SubscriptionManagement;

/// <summary>Platform data. Published versions are immutable (migration/DB guard); (Code, Version) unique.</summary>
public sealed class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans",
            t =>
            {
                t.HasCheckConstraint("ck_plans_version", "version >= 1");
                // PublishedAt is set when the plan is published and kept when it is retired (Draft is the only unpublished state).
                t.HasCheckConstraint("ck_plans_published_at", "(status = 'Draft') = (published_at IS NULL)");
            });
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Version).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(PlanStatus.Draft);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => new { x.Code, x.Version }).IsUnique();

        builder
            .HasMany(x => x.Limits)
            .WithOne()
            .HasForeignKey(l => l.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
        //builder
        //    .Navigation(x => x.Limits)
        //    .HasField("_limits")
        //    .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder
            .Navigation(x => x.Limits)
            .HasField("_limits")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .AutoInclude();   // bounded and needed by AddLimit / RemoveLimit

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
