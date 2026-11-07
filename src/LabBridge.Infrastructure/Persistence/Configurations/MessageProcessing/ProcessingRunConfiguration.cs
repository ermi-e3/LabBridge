using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.MessageIngestion.Entities;
using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;

/// <summary>At most one open run per message (partial unique index). Never stores message content.</summary>
public sealed class ProcessingRunConfiguration : IEntityTypeConfiguration<ProcessingRun>
{
    public void Configure(EntityTypeBuilder<ProcessingRun> builder)
    {
        builder.ToTable(
            "processing_runs",
            t =>
            {
                t.HasCheckConstraint(
                    "ck_processing_runs_outcome",
                    "(outcome IS NOT NULL) = (status = 'Completed')"
                );
                t.HasCheckConstraint(
                    "ck_processing_runs_scheduled",
                    "status <> 'Scheduled' OR next_attempt_at IS NOT NULL"
                );
                t.HasCheckConstraint("ck_processing_runs_in_progress",
                     "status <> 'InProgress' OR (worker_id IS NOT NULL AND started_at IS NOT NULL)");
            }
        );
        builder.HasDomainAssignedKey();
        builder.HasTenantAlternateKey();
        builder.Property(x => x.AttemptNumber).IsRequired();
        builder.Property(x => x.Trigger).HasEnumAsString(20);
        builder.Property(x => x.Status).HasEnumAsString(12);
        builder.Property(x => x.Outcome).HasEnumAsString(15);
        builder.Property(x => x.ReasonCode).HasMaxLength(60);
        builder.Property(x => x.MessageKind).HasEnumAsString(20);
        builder.Property(x => x.ParserVersion).HasMaxLength(30);
        builder.Property(x => x.WorkerId).HasMaxLength(100);
        builder.Property(x => x.ErrorCode).HasMaxLength(50);
        builder.Property(x => x.ErrorSummary).HasMaxLength(500);
        builder.Property(x => x.TraceId).HasMaxLength(64);

        builder.HasOrganizationOwner();
        builder
            .HasOne<InboundMessage>()
            .WithMany()
            .HasForeignKey(x => new { x.OrganizationId, x.MessageId })
            .HasPrincipalKey(m => new { m.OrganizationId, m.Id })
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<InterfaceProfileVersion>()
            .WithMany()
            .HasForeignKey(x => x.ProfileVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ProfileParserBinding>()
            .WithMany()
            .HasForeignKey(x => x.BindingId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ParserType>()
            .WithMany()
            .HasForeignKey(x => x.ParserTypeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.MessageId, x.AttemptNumber }).IsUnique();

        builder.HasAlternateKey(x => new { x.OrganizationId, x.MessageId, x.Id });

        builder
            .HasIndex(x => x.MessageId)
            .IsUnique()
            .HasFilter("status IN ('Scheduled','InProgress')")
            .HasDatabaseName("ux_processing_runs_open_per_message");
        builder.HasIndex(x => new { x.Status, x.NextAttemptAt });

        builder.IgnoreDomainEvents();
        builder.UseXminConcurrency();
        builder.UseSnakeCaseColumns();
    }
}
