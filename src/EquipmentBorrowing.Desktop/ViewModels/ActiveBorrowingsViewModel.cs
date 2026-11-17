
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class ActiveBorrowingsViewModel : ObservableObject
{
    private readonly IActiveBorrowingsReader _borrowingsReader;
    private readonly IAtomicBorrowingService _atomicBorrowingService;

    public ObservableCollection<Borrowing> ActiveBorrowings { get; }
        = new();

    [ObservableProperty]
    private Borrowing? selectedBorrowing;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public ActiveBorrowingsViewModel(
        IActiveBorrowingsReader borrowingsReader,
        IAtomicBorrowingService atomicBorrowingService)
    {
        _borrowingsReader = borrowingsReader;
        _atomicBorrowingService = atomicBorrowingService;
    }

    private static DateOnly GetManilaToday()
    {
        var timeZone =
            TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");

        var localNow = TimeZoneInfo.ConvertTime(
            DateTimeOffset.UtcNow,
            timeZone);

        return DateOnly.FromDateTime(localNow.DateTime);
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            var borrowings =
                await _borrowingsReader.GetActiveAsync();

            ActiveBorrowings.Clear();

            foreach (var borrowing in borrowings)
                ActiveBorrowings.Add(borrowing);

            SelectedBorrowing = null;
            StatusMessage = string.Empty;
        }
        catch (Exception)
        {
            StatusMessage =
                "Unable to load active borrowings. " +
                "Check the database connection and retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ReturnEquipmentAsync(Borrowing borrowing)
    {
        if (IsBusy)
            return;

        if (borrowing is null)
        {
            StatusMessage = "Please select an active borrowing.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _atomicBorrowingService.ReturnAsync(
                borrowing.StudentId,
                borrowing.EquipmentId,
                GetManilaToday());

            StatusMessage = result.Message;

            if (result.Status ==
                BorrowingOperationStatus.Success)
            {
                var refreshed =
                    await _borrowingsReader.GetActiveAsync();

                ActiveBorrowings.Clear();

                foreach (var item in refreshed)
                    ActiveBorrowings.Add(item);

                SelectedBorrowing = null;
            }
            else if (result.Status ==
                     BorrowingOperationStatus.OutcomeUnknown)
            {
                StatusMessage =
                    result.Message +
                    " Refresh the records before attempting " +
                    "another return.";
            }
        }
        catch (Exception)
        {
            StatusMessage =
                "Return did not complete normally. " +
                "Verify the borrowing records before retrying.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
