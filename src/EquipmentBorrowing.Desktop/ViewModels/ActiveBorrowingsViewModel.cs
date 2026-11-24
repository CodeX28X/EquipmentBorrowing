
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

    private bool _refreshRequired;

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
            _refreshRequired = false;
        }
        catch (Exception ex)
        {
            _refreshRequired = true;
            StatusMessage = ex is Services.HttpEquipmentBorrowingClient.ApiFailure
                ? ex.Message : "Unable to refresh records. Try refreshing again.";
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

        if (_refreshRequired)
        {
            StatusMessage = "Refresh this list successfully and verify the records before retrying.";
            return;
        }

        if (borrowing is null)
        {
            StatusMessage = "Please select an active borrowing.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _atomicBorrowingService.ReturnByIdAsync(
                borrowing.BorrowingId,
                GetManilaToday());

            StatusMessage = result.Message;
            _refreshRequired = result.ErrorKind != OperationErrorKind.Validation;

            if (result.Status ==
                BorrowingOperationStatus.Success)
            {
                var refreshed =
                    await _borrowingsReader.GetActiveAsync();

                ActiveBorrowings.Clear();

                foreach (var item in refreshed)
                    ActiveBorrowings.Add(item);

                SelectedBorrowing = null;
                _refreshRequired = false;
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
            _refreshRequired = true;
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
