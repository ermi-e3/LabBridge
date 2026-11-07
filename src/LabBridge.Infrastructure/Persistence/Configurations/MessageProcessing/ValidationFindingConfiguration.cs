using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;

/// <summary>No value-bearing column exists by design.</summary>
public sealed class ValidationFindingConfiguration : IEntityTypeConfiguration<ValidationFinding>
{
    public void Configure(EntityTypeBuilder<ValidationFinding> builder)
    {
        //builder.ToTable("validation_findings");
        builder.ToTable("validation_findings", "message_processing", t =>
              t.HasCheckConstraint("ck_validation_findings_counts", "occurrence_count >= 1 AND (line_number IS NULL OR line_number >= 1)"));
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Severity).HasEnumAsString(10);
        builder.Property(x => x.Category).HasEnumAsString(15);
        builder.Property(x => x.Code).HasMaxLength(60).IsRequired();
        builder.Property(x => x.FieldPath).HasMaxLength(300);
        builder.Property(x => x.OccurrenceCount).IsRequired().HasDefaultValue(1);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.HasOrganizationOwner();
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

        builder.HasIndex(x => new { x.OrganizationId, x.MessageId });
        builder.HasIndex(x => x.RunId);
        builder.UseSnakeCaseColumns();
    }
}
