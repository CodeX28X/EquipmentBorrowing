using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class SelectableEquipmentItem : ObservableObject
{
    public Equipment Equipment { get; }
    public string DisplayName =>
        $"{Equipment.EquipmentIdentifier} — {Equipment.EquipmentName}";
    public bool IsAvailable => Equipment.IsAvailable;

    public event Action<SelectableEquipmentItem>? SelectionChanged;

    [ObservableProperty]
    private bool isSelected;

    public SelectableEquipmentItem(Equipment equipment)
    {
        Equipment = equipment;
    }

    partial void OnIsSelectedChanged(bool value) =>
        SelectionChanged?.Invoke(this);
}

public partial class BorrowViewModel : ObservableObject
{
    private const int MaximumActiveBorrowings = 3;
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IActiveBorrowingsReader _activeBorrowingsReader;
    private readonly IAtomicBorrowingService _atomicBorrowingService;
    private readonly Dictionary<int, int> _studentActiveCounts = new();
    private bool _revertingSelection;

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<SelectableEquipmentItem> EquipmentChoices { get; } = new();

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private DateTime? expectedReturnDate;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    private bool _refreshRequired;

    public int SelectedCount => EquipmentChoices.Count(item => item.IsSelected);

    private int ActiveCount => SelectedStudent is not null &&
        _studentActiveCounts.TryGetValue(SelectedStudent.StudentId, out int count)
            ? count : 0;

    public int RemainingCapacity => Math.Max(0, MaximumActiveBorrowings - ActiveCount);

    public string CapacityMessage => SelectedStudent is null
        ? "Choose a student to see remaining borrowing capacity."
        : $"Active: {ActiveCount}/{MaximumActiveBorrowings}. " +
          $"Remaining slots: {RemainingCapacity}. Selected: {SelectedCount}.";

    public BorrowViewModel(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IActiveBorrowingsReader activeBorrowingsReader,
        IAtomicBorrowingService atomicBorrowingService)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _activeBorrowingsReader = activeBorrowingsReader;
        _atomicBorrowingService = atomicBorrowingService;
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        // Do not silently carry selections to a different student.
        foreach (var item in EquipmentChoices)
            item.IsSelected = false;

        NotifySelectionSummary();
    }

    private void NotifySelectionSummary()
    {
        OnPropertyChanged(nameof(SelectedCount));
        OnPropertyChanged(nameof(RemainingCapacity));
        OnPropertyChanged(nameof(CapacityMessage));
    }

    private void HandleSelectionChanged(SelectableEquipmentItem changed)
    {
        if (_revertingSelection)
            return;

        if (changed.IsSelected &&
            (!changed.IsAvailable ||
             SelectedStudent is null ||
             SelectedCount > RemainingCapacity))
        {
            _revertingSelection = true;
            changed.IsSelected = false;
            _revertingSelection = false;
            StatusMessage = SelectedStudent is null
                ? "Select a student before choosing equipment."
                : !changed.IsAvailable
                    ? "Unavailable equipment cannot be selected."
                    : "Selection exceeds the student's remaining borrowing slots.";
        }
        else
        {
            StatusMessage = string.Empty;
        }

        NotifySelectionSummary();
    }

    private static DateOnly GetManilaToday()
    {
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Manila");
        var localNow = TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, timeZone);
        return DateOnly.FromDateTime(localNow.DateTime);
    }

    private async Task RefreshDataAsync(bool resetStudent)
    {
        int? priorStudentId = resetStudent ? null : SelectedStudent?.StudentId;

        var students = await _studentRepository.GetAllAsync();
        var equipment = await _equipmentRepository.GetAllAsync();
        var activeBorrowings = await _activeBorrowingsReader.GetActiveAsync();

        _studentActiveCounts.Clear();
        foreach (var group in activeBorrowings.GroupBy(b => b.StudentId))
            _studentActiveCounts[group.Key] = group.Count();

        Students.Clear();
        foreach (var student in students)
            Students.Add(student);

        EquipmentChoices.Clear();
        foreach (var item in equipment)
        {
            var choice = new SelectableEquipmentItem(item);
            choice.SelectionChanged += HandleSelectionChanged;
            EquipmentChoices.Add(choice);
        }

        SelectedStudent = priorStudentId.HasValue
            ? Students.FirstOrDefault(s => s.StudentId == priorStudentId.Value)
            : null;

        NotifySelectionSummary();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            await RefreshDataAsync(resetStudent: true);
            ExpectedReturnDate = null;
            StatusMessage = "Select a student and 1–3 available items. Multiple items are checked out atomically.";
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
    private async Task BorrowEquipmentAsync()
    {
        if (IsBusy) return;

        if (_refreshRequired)
        {
            StatusMessage = "Refresh this list successfully and verify the records before retrying.";
            return;
        }

        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            return;
        }

        int[] equipmentIds = EquipmentChoices
            .Where(item => item.IsSelected)
            .Select(item => item.Equipment.EquipmentId)
            .ToArray();

        if (equipmentIds.Length == 0)
        {
            StatusMessage = "Select at least one available equipment item.";
            return;
        }

        if (equipmentIds.Length > RemainingCapacity)
        {
            StatusMessage = "Selection exceeds the student's remaining borrowing slots.";
            return;
        }

        if (ExpectedReturnDate is null)
        {
            StatusMessage = "Please select an expected return date.";
            return;
        }

        DateOnly checkoutDate = GetManilaToday();
        DateOnly returnDate = DateOnly.FromDateTime(ExpectedReturnDate.Value);
        if (returnDate <= checkoutDate)
        {
            StatusMessage = "Expected return date must be later than today.";
            return;
        }

        int studentId = SelectedStudent.StudentId;
        IsBusy = true;
        try
        {
            var result = await _atomicBorrowingService.CheckoutAsync(
                studentId, equipmentIds, checkoutDate, returnDate);

            StatusMessage = result.Message;
            _refreshRequired = result.ErrorKind != OperationErrorKind.Validation;

            if (result.Status == BorrowingOperationStatus.Success)
            {
                // Refresh availability and loan counts only after confirmed commit.
                ExpectedReturnDate = null;
                try
                {
                    await RefreshDataAsync(resetStudent: false);
                    _refreshRequired = false;
                    StatusMessage = $"{equipmentIds.Length} equipment item(s) checked out successfully.";
                }
                catch (Exception)
                {
                    StatusMessage = "Checkout committed successfully, but refreshing failed. " +
                        "Use Refresh Data to reload availability and active loans.";
                }
            }
            else if (result.Status == BorrowingOperationStatus.OutcomeUnknown)
            {
                StatusMessage = result.Message +
                    " Verify Active Borrowings before attempting another checkout.";
            }
            else
            {
                // Database is authoritative; after a conflict refresh before retrying.
                StatusMessage = result.Message + " Refresh data before retrying.";
            }
        }
        catch (Exception)
        {
            _refreshRequired = true;
            StatusMessage = "Checkout outcome could not be confirmed. " +
                "Verify Active Borrowings before attempting another checkout.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
