
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Tests.Domain;

public class EquipmentTests
{
    [Fact]
    public void NewEquipment_IsInitiallyAvailable()
    {
        var equipment = new Equipment(
            1, "LAP-001", "Laptop");

        Assert.Equal(1, equipment.EquipmentId);
        Assert.Equal("LAP-001", equipment.EquipmentIdentifier);
        Assert.Equal("Laptop", equipment.EquipmentName);
        Assert.True(equipment.IsAvailable);
        Assert.Null(equipment.Description);
    }

    [Theory]
    [InlineData("", "Laptop")]
    [InlineData("  ", "Laptop")]
    [InlineData("LAP-001", "")]
    [InlineData("LAP-001", "  ")]
    public void MissingRequiredFields_AreRejected(
        string identifier, string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Equipment(1, identifier, name));
    }

    [Fact]
    public void InvalidEquipmentId_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Equipment(0, "LAP-001", "Laptop"));
    }

    [Fact]
    public void EquipmentInformation_IsTrimmed()
    {
        var equipment = new Equipment(
            1, " LAP-001 ", " Laptop ", " Portable ");

        Assert.Equal("LAP-001", equipment.EquipmentIdentifier);
        Assert.Equal("Laptop", equipment.EquipmentName);
        Assert.Equal("Portable", equipment.Description);
    }

    [Fact]
    public void BlankDescription_IsStoredAsAbsent()
    {
        var equipment = new Equipment(
            1, "LAP-001", "Laptop", "  ");

        Assert.Null(equipment.Description);
    }

    [Fact]
    public void MarkAsBorrowed_MakesEquipmentUnavailable()
    {
        var equipment = new Equipment(
            1, "LAP-001", "Laptop");

        equipment.MarkAsBorrowed();

        Assert.False(equipment.IsAvailable);
    }

    [Fact]
    public void BorrowingUnavailableEquipment_IsRejected()
    {
        var equipment = new Equipment(
            1, "LAP-001", "Laptop");

        equipment.MarkAsBorrowed();

        Assert.Throws<InvalidOperationException>(() =>
            equipment.MarkAsBorrowed());
    }

    [Fact]
    public void ReturningBorrowedEquipment_RestoresAvailability()
    {
        var equipment = new Equipment(
            1, "LAP-001", "Laptop");

        equipment.MarkAsBorrowed();
        equipment.MarkAsAvailable();

        Assert.True(equipment.IsAvailable);
    }

    [Fact]
    public void ReturningAvailableEquipment_IsRejected()
    {
        var equipment = new Equipment(
            1, "LAP-001", "Laptop");

        Assert.Throws<InvalidOperationException>(() =>
            equipment.MarkAsAvailable());
    }

    [Fact]
    public void DescriptiveUpdate_PreservesAvailabilityAndIdentity()
    {
        var equipment = new Equipment(
            1, "LAP-001", "Laptop");

        equipment.MarkAsBorrowed();

        equipment.UpdateInformation(
            "LAP-002", "Updated Laptop", null);

        Assert.Equal(1, equipment.EquipmentId);
        Assert.Equal("LAP-002", equipment.EquipmentIdentifier);
        Assert.Equal("Updated Laptop", equipment.EquipmentName);
        Assert.False(equipment.IsAvailable);
    }

    [Fact]
    public void InvalidUpdate_DoesNotChangeExistingInformation()
    {
        var equipment = new Equipment(
            1, "LAP-001", "Laptop", "Portable");

        Assert.Throws<ArgumentException>(() =>
            equipment.UpdateInformation(
                "LAP-002", " ", null));

        Assert.Equal("LAP-001", equipment.EquipmentIdentifier);
        Assert.Equal("Laptop", equipment.EquipmentName);
        Assert.Equal("Portable", equipment.Description);
    }
}
