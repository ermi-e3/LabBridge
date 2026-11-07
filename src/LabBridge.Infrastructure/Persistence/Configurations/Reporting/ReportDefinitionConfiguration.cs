using LabBridge.Domain.Features.Reporting.Entities;
using LabBridge.Infrastructure.Persistence.Configurations.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Reporting;

public sealed class ReportDefinitionConfiguration : IEntityTypeConfiguration<ReportDefinition>
{
    public void Configure(EntityTypeBuilder<ReportDefinition> builder)
    {
        builder.ToTable("report_definitions");
        builder.HasKey(x => x.ReportCode);
        builder.Property(x => x.ReportCode).HasMaxLength(100).ValueGeneratedNever();
        builder.Property(x => x.Description).HasMaxLength(500).IsRequired();
        builder.Property(x => x.ParameterSchemaJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.SourcesJson).HasJsonbColumn().IsRequired();
        builder.Property(x => x.DimensionsJson).HasJsonbColumn().IsRequired();
        //builder.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        builder.Property(x => x.IsActive).IsRequired();   // no HasDefaultValue(true): the domain always assigns it
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.UseSnakeCaseColumns();
    }
}
