using EquipmentBorrowing.Domain.Entities;
using Xunit;

namespace EquipmentBorrowing.Tests.Domain;

public sealed class StudentYearRegressionTests
{
    [Fact]
    public void UpdateInformation_ChangesYear_WithoutResettingEligibility()
    {
        var student = new Student(123, "QA-ST-123", "Original", null, 1);
        student.SetBorrowingEligibility(false);

        student.UpdateInformation("QA-ST-123", "Updated", null, 4);

        Assert.Equal(4, student.StudentYear);
        Assert.False(student.IsAllowedToBorrow);
    }
}
