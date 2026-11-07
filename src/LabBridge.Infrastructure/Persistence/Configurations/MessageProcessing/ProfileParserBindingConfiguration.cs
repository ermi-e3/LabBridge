using LabBridge.Domain.Features.InstrumentManagement.Entities;
using LabBridge.Domain.Features.MessageProcessing.Entities;
using LabBridge.Domain.Features.MessageProcessing.Enums;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.MessageProcessing;

public sealed class ProfileParserBindingConfiguration
    : IEntityTypeConfiguration<ProfileParserBinding>
{
    public void Configure(EntityTypeBuilder<ProfileParserBinding> builder)
    {
        builder.ToTable("profile_parser_bindings", "message_processing");
        builder.HasDomainAssignedKey();
        builder.Property(x => x.ConfigJson).HasJsonbColumn();
        builder.Property(x => x.Status).HasEnumAsString(10).HasDefaultValue(BindingStatus.Active);
        builder.Property(x => x.CreatedAt).IsRequired();

        builder
            .HasOne<InterfaceProfileVersion>()
            .WithMany()
            .HasForeignKey(x => x.ProfileVersionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder
            .HasOne<ParserType>()
            .WithMany()
            .HasForeignKey(x => x.ParserTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => x.ProfileVersionId)
            .IsUnique()
            .HasFilter("status = 'Active'")
            .HasDatabaseName("ux_profile_parser_bindings_active");
        builder.UseSnakeCaseColumns();
    }
}
