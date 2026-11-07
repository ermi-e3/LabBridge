using LabBridge.Domain.Features.OrderResultManagement.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.OrderResultManagement;

public sealed class ResultStatusHistoryConfiguration : IEntityTypeConfiguration<ResultStatusHistory>
{
    public void Configure(EntityTypeBuilder<ResultStatusHistory> builder)
    {
        builder.ToTable("lab_result_status_history", "orderResult_management");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.FromStatus).HasEnumAsString(20);
        builder.Property(x => x.ToStatus).HasEnumAsString(20);
        builder.Property(x => x.Reason).HasMaxLength(500);
        builder.Property(x => x.ActorType).HasEnumAsString(10);
        builder.Property(x => x.Source).HasEnumAsString(10);
        builder.Property(x => x.ChangedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder.HasIndex(x => new { x.ResultId, x.ChangedAt });
        builder.UseSnakeCaseColumns();
    }
}
