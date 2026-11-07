using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;

/// <summary>Platform reference data. Supported kinds persisted as text[] from a private backing list.</summary>
public sealed class ParserTypeConfiguration : IEntityTypeConfiguration<ParserType>
{
    public void Configure(EntityTypeBuilder<ParserType> builder)
    {
        //builder.ToTable("parser_types");
        builder.ToTable("parser_types", t => t.HasCheckConstraint("ck_parser_types_kinds", "cardinality(supported_message_kinds) >= 1"));
        builder.HasDomainAssignedKey();
        builder.Property(x => x.Code).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Version).HasMaxLength(30).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.InputFormat).HasMaxLength(50).IsRequired();
        builder.Property(x => x.LimitsJson).HasJsonbColumn();
        builder.Property(x => x.CanonicalSchemaVersion).IsRequired();
        builder.Property(x => x.IsSimulator).IsRequired();
        //builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsActive).IsRequired();   // no HasDefaultValue(true): the domain always assigns it
        builder.Property(x => x.CreatedAt).IsRequired();

        builder.Ignore(x => x.SupportedMessageKinds);
        builder
            .Property<List<string>>("_supportedMessageKindNames")
            .HasColumnName("supported_message_kinds")
            .HasColumnType("text[]")
            .IsRequired();

        builder.HasIndex(x => new { x.Code, x.Version }).IsUnique();
        builder.UseSnakeCaseColumns();
    }
}
