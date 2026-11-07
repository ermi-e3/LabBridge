using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageIngestion;

public sealed class InboundMessageStatusHistoryConfiguration
    : IEntityTypeConfiguration<InboundMessageStatusHistory>
{
    public void Configure(EntityTypeBuilder<InboundMessageStatusHistory> builder)
    {
        builder.ToTable("inbound_message_status_history");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.FromStatus).HasEnumAsString(20);
        builder.Property(x => x.ToStatus).HasEnumAsString(20);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ActorType).HasEnumAsString(20);
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder.HasIndex(x => new { x.MessageId, x.ChangedAt });
        builder.UseSnakeCaseColumns();
    }
}
