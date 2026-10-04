using EquipmentBorrowing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EquipmentBorrowing.Infrastructure.Persistence.Configurations;

public sealed class EquipmentConfiguration : IEntityTypeConfiguration<Equipment>
{
    public void Configure(EntityTypeBuilder<Equipment> builder)
    {
        builder.HasKey(equipment => equipment.EquipmentId);

        builder.Property(equipment => equipment.EquipmentId)
            .ValueGeneratedNever();

        builder.Property(equipment => equipment.EquipmentName)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(equipment => equipment.IsAvailable)
            .IsRequired();

        builder.HasIndex(equipment => equipment.EquipmentName)
            .IsUnique();

        builder.HasIndex(equipment => equipment.IsAvailable);
    }
}