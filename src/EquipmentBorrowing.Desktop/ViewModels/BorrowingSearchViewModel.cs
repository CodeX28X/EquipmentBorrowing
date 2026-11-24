using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.ViewModels;

/// <summary>UC-09: database-side search, status filtering and stable sorting.</summary>
public partial class BorrowingSearchViewModel : ObservableObject
{
    private readonly IBorrowingSearchReader _reader;
    private readonly SemaphoreSlim _queryGate = new(1, 1);

    public ObservableCollection<Borrowing> Borrowings { get; } = new();

    [ObservableProperty] private string searchText = string.Empty;
    [ObservableProperty] private int selectedStatusIndex;
    [ObservableProperty] private int selectedSortFieldIndex;
    [ObservableProperty] private int selectedDirectionIndex = 1;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string statusMessage = "Choose criteria, then select Search.";

    public BorrowingSearchViewModel(IBorrowingSearchReader reader)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));
    }

    private BorrowingSearchCriteria GetCriteria() => new(
        SearchText,
        SelectedStatusIndex switch
        {
            1 => BorrowingSearchStatus.Active,
            2 => BorrowingSearchStatus.Returned,
            _ => BorrowingSearchStatus.All
        },
        SelectedSortFieldIndex == 1
            ? BorrowingSortField.ExpectedReturnDate
            : BorrowingSortField.CheckoutDate,
        SelectedDirectionIndex == 0
            ? BorrowingSortDirection.Ascending
            : BorrowingSortDirection.Descending);

    [RelayCommand]
    private Task LoadAsync() => SearchInternalAsync();

    [RelayCommand]
    private Task SearchAsync() => SearchInternalAsync();

    [RelayCommand]
    private Task ClearAsync()
    {
        SearchText = string.Empty;
        SelectedStatusIndex = 0;
        SelectedSortFieldIndex = 0;
        SelectedDirectionIndex = 1;
        return SearchInternalAsync();
    }

    // One query at a time; controls do not permit overlapping search and clear
    // operations. A fresh query is executed each time the view is reopened.
    private async Task SearchInternalAsync()
    {
        if (!await _queryGate.WaitAsync(0)) return;

        IsBusy = true;
        StatusMessage = "Loading matching borrowings...";

        try
        {
            var results = await _reader.SearchAsync(GetCriteria());
            Borrowings.Clear();
            foreach (var record in results) Borrowings.Add(record);

            StatusMessage = results.Count == 0
                ? "No borrowings match the selected criteria."
                : $"{results.Count} matching borrowing record(s).";
        }
        catch (Exception ex)
        {
            // Clear stale results: do not display them as if they matched
            // a query that failed to load.
            Borrowings.Clear();
            StatusMessage = ex is Services.HttpEquipmentBorrowingClient.ApiFailure ? ex.Message : "Unable to load borrowings from the API. " +
                            "Check the connection and choose Search to retry.";
        }
        finally
        {
            IsBusy = false;
            _queryGate.Release();
        }
    }
}
