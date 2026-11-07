using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;

public sealed class ProcessingReviewConfiguration : IEntityTypeConfiguration<ProcessingReview>
{
    public void Configure(EntityTypeBuilder<ProcessingReview> builder)
    {
        builder.ToTable("processing_reviews", "message_processing");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Decision).HasEnumAsString(10);
        builder.Property(x => x.Reason).HasMaxLength(500).IsRequired();
        builder.Property(x => x.DecidedBy).IsRequired();
        builder.Property(x => x.DecidedAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<InboundMessage>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);
        //builder
        //    .HasOne<ProcessingRun>()
        //    .WithMany()
        //    .HasForeignKey(x => new { x.OrganizationId, x.RunId })
        //    .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
        //    .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne<ProcessingRun>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MessageId, x.RunId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.MessageId, r.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MessageId, x.DecidedAt });
        builder.UseSnakeCaseColumns();
    }
}
