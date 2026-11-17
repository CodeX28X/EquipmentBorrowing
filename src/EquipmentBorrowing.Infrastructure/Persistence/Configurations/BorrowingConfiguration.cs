
using EquipmentBorrowing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class BorrowingConfiguration
    : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.ToTable("Borrowings", table =>
        {
            table.HasCheckConstraint(
                "CK_Borrowings_ExpectedReturnDate",
                "\"ExpectedReturnDate\" > \"BorrowedAt\"");

            table.HasCheckConstraint(
                "CK_Borrowings_ActualReturnDate",
                "\"ActualReturnDate\" IS NULL OR " +
                "\"ActualReturnDate\" >= \"BorrowedAt\"");

            table.HasCheckConstraint(
                "CK_Borrowings_Status",
                "\"Status\" IN (0, 1)");

            table.HasCheckConstraint(
                "CK_Borrowings_Status_ReturnDate",
                "(\"Status\" = 0 AND \"ActualReturnDate\" IS NULL) OR " +
                "(\"Status\" = 1 AND \"ActualReturnDate\" IS NOT NULL)");
        });

        builder.HasKey(b => b.BorrowingId);

        builder.Property(b => b.BorrowingId)
            .ValueGeneratedOnAdd();

        builder.Property(b => b.StudentId)
            .IsRequired();

        builder.Property(b => b.EquipmentId)
            .IsRequired();

        builder.Property(b => b.BorrowedAt)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(b => b.ExpectedReturnDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(b => b.ActualReturnDate)
            .HasColumnType("date")
            .IsRequired(false);

        // Existing enum: Active = 0, Returned = 1.
        builder.Property(b => b.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasOne(b => b.Student)
            .WithMany()
            .HasForeignKey(b => b.StudentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        builder.HasOne(b => b.Equipment)
            .WithMany()
            .HasForeignKey(b => b.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();

        // Prevent two active borrowings for one physical item.
        builder.HasIndex(b => b.EquipmentId)
            .IsUnique()
            .HasFilter("\"Status\" = 0")
            .HasDatabaseName("UX_Borrowings_ActiveEquipment");

        builder.HasIndex(b => new
        {
            b.StudentId,
            b.BorrowedAt
        });

        builder.HasIndex(b => new
        {
            b.EquipmentId,
            b.BorrowedAt
        });

        builder.HasIndex(b => new
        {
            b.Status,
            b.ExpectedReturnDate
        });

        builder.HasIndex(b => b.BorrowedAt);
    }
}
