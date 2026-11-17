
using EquipmentBorrowing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class EquipmentConfiguration
    : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipment", table =>
        {
            table.HasCheckConstraint(
                "CK_Equipment_Identifier_NotBlank",
                "length(btrim(\"EquipmentIdentifier\")) > 0");

            table.HasCheckConstraint(
                "CK_Equipment_Name_NotBlank",
                "length(btrim(\"EquipmentName\")) > 0");

            table.HasCheckConstraint(
                "CK_Equipment_Description_NotBlank",
                "\"Description\" IS NULL OR " +
                "length(btrim(\"Description\")) > 0");
        });

        builder.HasKey(e => e.EquipmentId);

        // Preserve existing application-assigned IDs.
        builder.Property(e => e.EquipmentId)
            .ValueGeneratedNever();

        builder.Property(e => e.EquipmentIdentifier)
            .IsRequired();

        builder.Property(e => e.EquipmentName)
            .IsRequired();

        builder.Property(e => e.Description)
            .IsRequired(false);

        builder.Property(e => e.IsAvailable)
            .IsRequired()
            .HasDefaultValue(true);


        builder.Property<string>("NormalizedEquipmentIdentifier")
            .HasComputedColumnSql(
                "lower(regexp_replace(\"EquipmentIdentifier\", " +
                "'^[[:space:]]+|[[:space:]]+$', '', 'g'))",
                stored: true);


        builder.HasIndex("NormalizedEquipmentIdentifier")
            .IsUnique()
            .HasDatabaseName("UX_Equipment_NormalizedIdentifier");

        builder.HasIndex(e => e.EquipmentName);

        builder.HasIndex(e => e.IsAvailable);
    }
}
