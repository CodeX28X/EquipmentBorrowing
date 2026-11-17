
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Domain.Enums;
using EquipmentBorrowing.Infrastructure.Repositories;

namespace EquipmentBorrowing.Tests;

public class ReturnEquipmentServiceTests
{
    private static (Student Student, Equipment Item,
        Borrowing Loan, InMemoryBorrowingRepository Repo)
        CreateActiveLoan()
    {
        var student = new Student(
            1, "STU-001", "Juan Dela Cruz");

        var item = new Equipment(
            101, "LAP-101", "Laptop");

        var loan = new Borrowing(
            student,
            item,
            new DateOnly(2025, 1, 10),
            new DateOnly(2025, 1, 17));

        item.MarkAsBorrowed();

        var repo = new InMemoryBorrowingRepository(
            new[] { loan });

        return (student, item, loan, repo);
    }

    [Fact]
    public async Task ActiveLoan_ReturnsSuccessfully()
    {
        var data = CreateActiveLoan();
        var service = new ReturnEquipmentService(data.Repo);

        var result = await service.ReturnAsync(
            data.Student.StudentId, data.Item.EquipmentId);

        Assert.True(result.IsSuccess);
        Assert.Equal("Equipment returned successfully.",
            result.Message);
        Assert.Same(data.Loan, result.Borrowing);
    }

    [Fact]
    public async Task Return_RecordsDateAndStatus()
    {
        var data = CreateActiveLoan();
        var service = new ReturnEquipmentService(data.Repo);

        await service.ReturnAsync(1, 101);

        Assert.Equal(BorrowingStatus.Returned,
            data.Loan.Status);
        Assert.NotNull(data.Loan.ActualReturnDate);
        Assert.True(data.Loan.ActualReturnDate
            >= data.Loan.BorrowedAt);
    }

    [Fact]
    public async Task Return_MakesEquipmentAvailable()
    {
        var data = CreateActiveLoan();
        var service = new ReturnEquipmentService(data.Repo);

        await service.ReturnAsync(1, 101);

        Assert.True(data.Item.IsAvailable);
        Assert.Empty(await data.Repo.GetActiveAsync());
    }

    [Fact]
    public async Task MissingActiveLoan_IsRejected()
    {
        var repo = new InMemoryBorrowingRepository();
        var service = new ReturnEquipmentService(repo);

        var result = await service.ReturnAsync(999, 999);

        Assert.False(result.IsSuccess);
        Assert.Null(result.Borrowing);
    }

    [Fact]
    public async Task RepeatReturn_IsRejectedWithoutChanges()
    {
        var data = CreateActiveLoan();
        var service = new ReturnEquipmentService(data.Repo);

        var first = await service.ReturnAsync(1, 101);
        var originalDate = data.Loan.ActualReturnDate;

        var second = await service.ReturnAsync(1, 101);

        Assert.True(first.IsSuccess);
        Assert.False(second.IsSuccess);
        Assert.Equal(originalDate,
            data.Loan.ActualReturnDate);
        Assert.True(data.Item.IsAvailable);
    }

    [Fact]
    public async Task RestrictedStudent_CanReturnExistingLoan()
    {
        var data = CreateActiveLoan();
        data.Student.SetBorrowingEligibility(false);
        var service = new ReturnEquipmentService(data.Repo);

        var result = await service.ReturnAsync(1, 101);

        Assert.True(result.IsSuccess);
        Assert.False(data.Student.IsAllowedToBorrow);
        Assert.True(data.Item.IsAvailable);
    }
}
