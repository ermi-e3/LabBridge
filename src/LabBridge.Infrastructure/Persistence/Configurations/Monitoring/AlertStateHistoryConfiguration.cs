using LabBridge.Domain.Features.Monitoring.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Monitoring;

public sealed class AlertStateHistoryConfiguration : IEntityTypeConfiguration<AlertStateHistory>
{
    public void Configure(EntityTypeBuilder<AlertStateHistory> builder)
    {
        builder.ToTable("alert_state_history");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.FromStatus).HasEnumAsString(15);
        builder.Property(x => x.ToStatus).HasEnumAsString(15);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasIndex(x => new { x.AlertId, x.ChangedAt });
        builder.UseSnakeCaseColumns();
    }
}
