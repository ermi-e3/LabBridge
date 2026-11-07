using System.Text;
using LabBridge.Domain.Common;
using LabBridge.Domain.Features.OrganizationManagement.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LabBridge.Infrastructure.Persistence.Configurations.Common;

/// <summary>
/// Configuration helpers shared by all entity configurations. They contain mapping conventions only (no business logic).
/// </summary>
public static class EntityTypeBuilderExtensions
{
    /// <summary>
    /// Optimistic concurrency through the PostgreSQL system column xmin (FRS "concurrency token surfaced as ETag").
    /// Mapped as a shadow property so the domain stays free of persistence concerns.
    /// </summary>
    public static EntityTypeBuilder<T> UseXminConcurrency<T>(this EntityTypeBuilder<T> builder)
        where T : class
    {
        builder
            .Property<uint>("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();
        return builder;
    }

    /// <summary>Required FK to Organization (tenant root), ON DELETE RESTRICT (SC-ORG-003).</summary>
    public static EntityTypeBuilder<T> HasOrganizationOwner<T>(this EntityTypeBuilder<T> builder)
        where T : class, IOrganizationOwned
    {
        builder.Property(nameof(IOrganizationOwned.OrganizationId)).IsRequired();
        builder
            .HasOne<Organization>()
            .WithMany()
            .HasForeignKey(nameof(IOrganizationOwned.OrganizationId))
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
        return builder;
    }

    /// <summary>Aggregate roots raise domain events in memory; they are never persisted.</summary>
    public static EntityTypeBuilder<T> IgnoreDomainEvents<T>(this EntityTypeBuilder<T> builder)
        where T : AggregateRoot
    {
        builder.Ignore(x => x.DomainEvents);
        return builder;
    }

    /// <summary>Keys are assigned by the domain; the database never generates them.</summary>
    public static EntityTypeBuilder<T> HasDomainAssignedKey<T>(this EntityTypeBuilder<T> builder)
        where T : Entity
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        return builder;
    }

    /// <summary>Unique (OrganizationId, Id) key used as the target of composite tenant foreign keys (SC-ORG-003).</summary>
    public static EntityTypeBuilder<T> HasTenantAlternateKey<T>(this EntityTypeBuilder<T> builder)
        where T : Entity, IOrganizationOwned
    {
        builder.HasAlternateKey(nameof(IOrganizationOwned.OrganizationId), nameof(Entity.Id));
        return builder;
    }

    /// <summary>Stores a JSON document in a PostgreSQL jsonb column (the domain keeps the JSON text).</summary>
    public static PropertyBuilder<string> HasJsonbColumn(this PropertyBuilder<string> property) =>
        property.HasColumnType("jsonb");

    /// <summary>Case-insensitive text (PostgreSQL citext; the extension must be enabled by the host DbContext/migrations).</summary>
    public static PropertyBuilder<string> HasCaseInsensitiveText(
        this PropertyBuilder<string> property,
        int maxLength
    ) => property.HasColumnType("citext").HasMaxLength(maxLength);

    /// <summary>Stores an enum as its name (stable, readable, migration-friendly).</summary>
    public static PropertyBuilder<TEnum> HasEnumAsString<TEnum>(
        this PropertyBuilder<TEnum> property,
        int maxLength
    )
        where TEnum : struct, Enum =>
        property.HasConversion<string>().HasMaxLength(maxLength).IsRequired();

    public static PropertyBuilder<TEnum?> HasEnumAsString<TEnum>(
        this PropertyBuilder<TEnum?> property,
        int maxLength
    )
        where TEnum : struct, Enum => property.HasConversion<string>().HasMaxLength(maxLength);

    /// <summary>
    /// Applies snake_case column names to every property declared on the entity (PostgreSQL convention). MUST be the last call in Configure
    /// so that all properties exist; raw SQL used in filters/check constraints refers to these names.
    /// </summary>
    public static EntityTypeBuilder<T> UseSnakeCaseColumns<T>(this EntityTypeBuilder<T> builder)
        where T : class
    {
        foreach (var property in builder.Metadata.GetProperties())
        {
            // Skip xmin and any column name that was configured explicitly.
            if (
                property.Name == "xmin"
                || property.FindAnnotation(RelationalAnnotationNames.ColumnName) is not null
            )
                continue;
            property.SetColumnName(ToSnakeCase(property.Name));
        }
        return builder;
    }

    internal static string ToSnakeCase(string name)
    {
        var sb = new StringBuilder(name.Length + 4);
        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];
            if (char.IsUpper(c))
            {
                var prevLower = i > 0 && !char.IsUpper(name[i - 1]);
                var nextLower = i + 1 < name.Length && char.IsLower(name[i + 1]);
                if (i > 0 && (prevLower || nextLower))
                    sb.Append('_');
                sb.Append(char.ToLowerInvariant(c));
            }
            else
            {
                sb.Append(c);
            }
        }
        return sb.ToString();
    }
}
