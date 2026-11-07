using LabBridge.Domain.Features.TestCodeMapping.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.TestCodeMapping;

public sealed class MappingGapSubjectConfiguration : IEntityTypeConfiguration<MappingGapSubject>
{
    public void Configure(EntityTypeBuilder<MappingGapSubject> builder)
    {
        builder.ToTable("mapping_gap_subjects", "testCode_mapping");
        builder.HasKey(x => new
        {
            x.GapId,
            x.SubjectType,
            x.SubjectId,
        });
        builder.Property(x => x.SubjectType).HasEnumAsString(10);
        builder.Property(x => x.FirstSeenAt).IsRequired();

        builder.HasOrganizationOwner();
        builder
            .HasOne<MappingGap>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.GapId })
            .HasPrincipalKey(g => new { g.OrganizationId, g.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<MappingRun>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.RunId })
            .HasPrincipalKey(r => new { r.OrganizationId, r.Id })
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SubjectType, x.SubjectId });
        builder.UseSnakeCaseColumns();
    }
}
