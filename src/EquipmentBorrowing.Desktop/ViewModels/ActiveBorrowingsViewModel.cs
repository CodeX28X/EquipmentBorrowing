using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class ActiveBorrowingsViewModel : ObservableObject
{
    private readonly IBorrowingRepository _borrowingRepository;
    private readonly ReturnEquipmentService _returnEquipmentService;

    public ObservableCollection<Borrowing> ActiveBorrowings { get; } = new();

    [ObservableProperty]
    private Borrowing? selectedBorrowing;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    public ActiveBorrowingsViewModel(
        IBorrowingRepository borrowingRepository,
        ReturnEquipmentService returnEquipmentService)
    {
        _borrowingRepository = borrowingRepository;
        _returnEquipmentService = returnEquipmentService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IReadOnlyList<Borrowing> borrowings =
            await _borrowingRepository.GetActiveAsync();

        ActiveBorrowings.Clear();

        foreach (Borrowing borrowing in borrowings)
        {
            ActiveBorrowings.Add(borrowing);
        }
    }

    [RelayCommand]
    private async Task ReturnEquipmentAsync(Borrowing borrowing)
    {
        ReturnResult result =
            await _returnEquipmentService.ReturnAsync(
                borrowing.Student.StudentId,
                borrowing.Equipment.EquipmentId);

        StatusMessage = result.Message;

        if (result.IsSuccess)
        {
            await LoadAsync();
        }
    }
}