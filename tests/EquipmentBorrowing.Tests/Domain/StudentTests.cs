
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Tests.Domain;

public class StudentTests
{
    [Fact]
    public void ValidStudent_HasRequiredInformation()
    {
        var student = new Student(
            1, "STU-001", "Juan Dela Cruz",
            "09123456789", 2);

        Assert.Equal(1, student.StudentId);
        Assert.Equal("STU-001", student.StudentNumber);
        Assert.Equal("Juan Dela Cruz", student.StudentName);
        Assert.Equal("09123456789", student.ContactInformation);
        Assert.Equal(2, student.StudentYear);
        Assert.True(student.IsAllowedToBorrow);
    }

    [Fact]
    public void NewStudent_IsEligibleByDefault()
    {
        var student = new Student(
            1, "STU-001", "Juan Dela Cruz");

        Assert.True(student.IsAllowedToBorrow);
        Assert.Null(student.ContactInformation);
        Assert.Null(student.StudentYear);
    }

    [Theory]
    [InlineData("", "Juan")]
    [InlineData("   ", "Juan")]
    [InlineData("STU-001", "")]
    [InlineData("STU-001", "   ")]
    public void MissingRequiredData_IsRejected(
        string number, string name)
    {
        Assert.Throws<ArgumentException>(() =>
            new Student(1, number, name));
    }

    [Fact]
    public void InvalidInternalId_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Student(0, "STU-001", "Juan"));
    }

    [Fact]
    public void InvalidYear_IsRejected()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Student(1, "STU-001", "Juan",
                studentYear: 0));
    }

    [Fact]
    public void Information_IsTrimmed()
    {
        var student = new Student(
            1, "  STU-001  ", "  Juan Dela Cruz  ",
            "  09123456789  ");

        Assert.Equal("STU-001", student.StudentNumber);
        Assert.Equal("Juan Dela Cruz", student.StudentName);
        Assert.Equal("09123456789", student.ContactInformation);
    }

    [Fact]
    public void BlankOptionalContact_IsStoredAsAbsent()
    {
        var student = new Student(
            1, "STU-001", "Juan", "   ");

        Assert.Null(student.ContactInformation);
    }

    [Fact]
    public void UpdatingInformation_PreservesInternalIdentity()
    {
        var student = new Student(
            1, "STU-001", "Juan");

        student.UpdateInformation(
            "STU-002", "Juan Dela Cruz", null);

        Assert.Equal(1, student.StudentId);
        Assert.Equal("STU-002", student.StudentNumber);
        Assert.Equal("Juan Dela Cruz", student.StudentName);
    }

    [Fact]
    public void InvalidUpdate_PreservesExistingInformation()
    {
        var student = new Student(
            1, "STU-001", "Juan", "123");

        Assert.Throws<ArgumentException>(() =>
            student.UpdateInformation(
                "STU-002", " ", null));

        Assert.Equal("STU-001", student.StudentNumber);
        Assert.Equal("Juan", student.StudentName);
        Assert.Equal("123", student.ContactInformation);
    }

    [Fact]
    public void Eligibility_CanBeRestrictedAndRestored()
    {
        var student = new Student(
            1, "STU-001", "Juan");

        student.SetBorrowingEligibility(false);
        Assert.False(student.IsAllowedToBorrow);

        student.SetBorrowingEligibility(true);
        Assert.True(student.IsAllowedToBorrow);
    }

    [Fact]
    public void RepeatingEligibilityState_IsHarmless()
    {
        var student = new Student(
            1, "STU-001", "Juan");

        student.SetBorrowingEligibility(true);

        Assert.True(student.IsAllowedToBorrow);
    }
}
