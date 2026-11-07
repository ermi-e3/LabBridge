using LabBridge.Domain.Features.SubscriptionManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.SubscriptionManagement;

public sealed class LimitDefinitionConfiguration : IEntityTypeConfiguration<LimitDefinition>
{
    public void Configure(EntityTypeBuilder<LimitDefinition> builder)
    {
        builder.ToTable("limit_definitions");
        builder.Property(x => x.LimitKey).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(x => x.Kind).HasEnumAsString(10);
        builder.Property(x => x.Unit).HasMaxLength(50);
        builder.Property(x => x.MetricSource).HasMaxLength(100);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.IsMessageVolume).IsRequired();
        //builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsActive).IsRequired();   // no HasDefaultValue(true): the domain always assigns it

        builder.HasKey(x => x.LimitKey);
        builder.HasAlternateKey(x => new { x.LimitKey, x.Kind, x.IsMessageVolume });   // target of the composite foreign key
        builder.UseSnakeCaseColumns();
    }
}
