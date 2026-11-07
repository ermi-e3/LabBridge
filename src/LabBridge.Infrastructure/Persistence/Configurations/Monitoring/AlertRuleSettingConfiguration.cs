using LabBridge.Domain.Features.Monitoring.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Monitoring;

public sealed class AlertRuleSettingConfiguration : IEntityTypeConfiguration<AlertRuleSetting>
{
    public void Configure(EntityTypeBuilder<AlertRuleSetting> builder)
    {
        builder.ToTable("alert_rule_settings");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Enabled).IsRequired();
        builder.Property(x => x.Severity).HasEnumAsString(10);
        builder.Property(x => x.ThresholdsJson).HasJsonbColumn();
        builder.Property(x => x.UpdatedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<AlertRule>()
            .WithMany()
            .HasForeignKey(x => x.AlertRuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.OrganizationId, x.AlertRuleId }).IsUnique();
        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
