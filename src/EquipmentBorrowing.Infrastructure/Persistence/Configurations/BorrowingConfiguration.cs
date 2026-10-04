using EquipmentBorrowing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class BorrowingConfiguration : IEntityTypeConfiguration<Borrowing>
{
    public void Configure(EntityTypeBuilder<Borrowing> builder)
    {
        builder.Property<int>("BorrowingId")
            .ValueGeneratedOnAdd();

        builder.HasKey("BorrowingId");

        builder.Property(borrowing => borrowing.BorrowedAt)
            .IsRequired();

        builder.Property(borrowing => borrowing.ExpectedReturnDate)
            .IsRequired();

        builder.Property(borrowing => borrowing.Status)
            .IsRequired();

        builder.HasOne(borrowing => borrowing.Student)
            .WithMany()
            .HasForeignKey("StudentId")
            .IsRequired();

        builder.HasOne(borrowing => borrowing.Equipment)
            .WithMany()
            .HasForeignKey("EquipmentId")
            .IsRequired();

        builder.HasIndex("StudentId");

        builder.HasIndex("EquipmentId");

        builder.HasIndex(borrowing => borrowing.Status);
    }
}