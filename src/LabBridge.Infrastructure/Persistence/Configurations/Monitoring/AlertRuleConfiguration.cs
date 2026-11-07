using LabBridge.Domain.Features.Monitoring.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Monitoring;

public sealed class AlertRuleConfiguration : IEntityTypeConfiguration<AlertRule>
{
    public void Configure(EntityTypeBuilder<AlertRule> builder)
    {
        builder.ToTable("alert_rules", "monitoring");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.RuleCode).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Scope).HasEnumAsString(15);
        builder.Property(x => x.Kind).HasEnumAsString(20);
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DefaultSeverity).HasEnumAsString(10);
        builder.Property(x => x.ParameterSchemaJson).HasJsonbColumn();
        //builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsActive).IsRequired();   // no HasDefaultValue(true): the domain always assigns it
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasIndex(x => x.RuleCode).IsUnique();
        builder.UseSnakeCaseColumns();
    }
}
