using LabBridge.Domain.Features.Audit.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Audit;

/// <summary>Reference data keyed by the action code (natural key).</summary>
public sealed class AuditActionDefinitionConfiguration
    : IEntityTypeConfiguration<AuditActionDefinition>
{
    public void Configure(EntityTypeBuilder<AuditActionDefinition> builder)
    {
        builder.ToTable("audit_action_definitions","audit");
        builder.HasKey(x => x.ActionCode);
        builder.Property(x => x.ActionCode).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(x => x.Domain).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ActionClass).HasEnumAsString(15);
        builder.Property(x => x.FailureMode).HasEnumAsString(15);
        //builder.Property(x => x.DetailSchemaJson).HasJsonbColumn();
        builder.Property(x => x.DetailSchemaJson).HasColumnType("json");
        builder.Property(x => x.IsTraced).IsRequired();
        builder.Property(x => x.TraceStage).HasMaxLength(50);
        //builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsActive).IsRequired();   // no HasDefaultValue(true): the domain always assigns it
        builder.Property(x => x.CreatedAt).IsRequired();

        // change 
        builder.ToTable( t => t.HasCheckConstraint("ck_audit_action_definitions_trace", "is_traced = (trace_stage IS NOT NULL)"));
        builder.UseSnakeCaseColumns();
    }
}
