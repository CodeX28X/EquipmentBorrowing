using EquipmentBorrowing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(student => student.StudentId);

        builder.Property(student => student.StudentId)
            .ValueGeneratedNever();

        builder.Property(student => student.StudentName)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(student => student.StudentYear)
            .IsRequired();

        builder.Property(student => student.IsAllowedToBorrow)
            .IsRequired();

        builder.HasIndex(student => student.StudentName)
            .IsUnique();
    }
}