
using EquipmentBorrowing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration
    : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.ToTable("Students", table =>
        {
            table.HasCheckConstraint(
                "CK_Students_StudentNumber_NotBlank",
                "length(btrim(\"StudentNumber\")) > 0");

            table.HasCheckConstraint(
                "CK_Students_StudentName_NotBlank",
                "length(btrim(\"StudentName\")) > 0");

            table.HasCheckConstraint(
                "CK_Students_Contact_NotBlank",
                "\"ContactInformation\" IS NULL OR " +
                "length(btrim(\"ContactInformation\")) > 0");
        });

        builder.HasKey(s => s.StudentId);

        // Preserve Lab 3's application-assigned integer IDs.
        builder.Property(s => s.StudentId)
            .ValueGeneratedNever();

        builder.Property(s => s.StudentNumber)
            .IsRequired();

        builder.Property(s => s.StudentName)
            .IsRequired();

        builder.Property(s => s.ContactInformation)
            .IsRequired(false);

        builder.Property(s => s.StudentYear)
            .IsRequired(false);

        builder.Property(s => s.IsAllowedToBorrow)
            .IsRequired()
            .HasDefaultValue(true);

        // PostgreSQL computes the normalized identifier.

        builder.Property<string>("NormalizedStudentNumber")
            .HasComputedColumnSql(
                "lower(regexp_replace(\"StudentNumber\", " +
                "'^[[:space:]]+|[[:space:]]+$', '', 'g'))",
                stored: true);


        builder.HasIndex("NormalizedStudentNumber")
            .IsUnique()
            .HasDatabaseName("UX_Students_NormalizedStudentNumber");

        builder.HasIndex(s => s.StudentName);
    }
}
