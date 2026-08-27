using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Tests.Domain;

public class EquipmentTests
{
    [Fact]
    public void Constructor_WithValidInformation_CreatesAvailableEquipment()
    {
        var equipment = new Equipment(
            1,
            "Laptop");

        Assert.Equal(1, equipment.EquipmentId);
        Assert.Equal("Laptop", equipment.EquipmentName);
        Assert.True(equipment.IsAvailable);
    }

    [Fact]
    public void MarkAsBorrowed_WhenEquipmentIsAvailable_MakesEquipmentUnavailable()
    {
        var equipment = new Equipment(
            1,
            "Laptop");

        equipment.MarkAsBorrowed();

        Assert.False(equipment.IsAvailable);
    }

    [Fact]
    public void MarkAsBorrowed_WhenEquipmentIsAlreadyBorrowed_ThrowsException()
    {
        var equipment = new Equipment(
            1,
            "Laptop");

        equipment.MarkAsBorrowed();

        Assert.Throws<InvalidOperationException>(() =>
            equipment.MarkAsBorrowed());
    }

    [Fact]
    public void MarkAsAvailable_WhenEquipmentIsBorrowed_MakesEquipmentAvailable()
    {
        var equipment = new Equipment(
            1,
            "Laptop");

        equipment.MarkAsBorrowed();
        equipment.MarkAsAvailable();

        Assert.True(equipment.IsAvailable);
    }

    [Fact]
    public void Constructor_WithInvalidEquipmentId_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Equipment(
                0,
                "Laptop"));
    }

    [Fact]
    public void Constructor_WithEmptyEquipmentName_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() =>
            new Equipment(
                1,
                ""));
    }
}