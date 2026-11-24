using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.ViewModels;

public enum BorrowingHistoryMode { Student, Equipment }

/// <summary>One read-only ViewModel used by the two history destinations.</summary>
public partial class BorrowingHistoryViewModel : ObservableObject
{
    private readonly IBorrowingHistoryReader _historyReader;
    private readonly IStudentRepository _studentReader;
    private readonly IEquipmentRepository _equipmentReader;
    private int _requestVersion;

    public bool IsStudentMode { get; }
    public bool IsEquipmentMode => !IsStudentMode;
    public string PageTitle => IsStudentMode
        ? "Student Borrowing History" : "Equipment Borrowing History";
    public string Instruction => IsStudentMode
        ? "Select a student to see every active and returned loan."
        : "Select equipment to see every student who borrowed it.";

    public ObservableCollection<Student> Students { get; } = new();
    public ObservableCollection<Equipment> EquipmentItems { get; } = new();
    public ObservableCollection<Borrowing> History { get; } = new();

    [ObservableProperty] private Student? selectedStudent;
    [ObservableProperty] private Equipment? selectedEquipment;
    [ObservableProperty] private string statusMessage = "Select a record to view its history.";
    [ObservableProperty] private bool isBusy;

    public BorrowingHistoryViewModel(
        BorrowingHistoryMode mode,
        IBorrowingHistoryReader historyReader,
        IStudentRepository studentReader,
        IEquipmentRepository equipmentReader)
    {
        IsStudentMode = mode == BorrowingHistoryMode.Student;
        _historyReader = historyReader;
        _studentReader = studentReader;
        _equipmentReader = equipmentReader;
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        if (IsStudentMode) _ = RefreshHistoryAsync();
    }

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        if (IsEquipmentMode) _ = RefreshHistoryAsync();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        // Invalidates any older pending history query before updating selectors.
        Interlocked.Increment(ref _requestVersion);
        IsBusy = true;
        StatusMessage = "Loading records...";
        try
        {
            if (IsStudentMode)
            {
                int? previousId = SelectedStudent?.StudentId;
                var students = await _studentReader.GetAllAsync();
                Students.Clear();
                foreach (var student in students) Students.Add(student);
                SelectedStudent = Students.FirstOrDefault(s => s.StudentId == previousId);
            }
            else
            {
                int? previousId = SelectedEquipment?.EquipmentId;
                var items = await _equipmentReader.GetAllAsync();
                EquipmentItems.Clear();
                foreach (var item in items) EquipmentItems.Add(item);
                SelectedEquipment = EquipmentItems.FirstOrDefault(e => e.EquipmentId == previousId);
            }

            if (IsStudentMode ? SelectedStudent is null : SelectedEquipment is null)
            {
                History.Clear();
                StatusMessage = "Select a record to view its history.";
                IsBusy = false;
            }
            // Otherwise the selection-change callback starts the history query.
        }
        catch (Exception)
        {
            History.Clear();
            StatusMessage = "Unable to load the selection list. Check that the API is running and retry.";
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RefreshHistoryAsync()
    {
        int request = Interlocked.Increment(ref _requestVersion);
        int? selectedId = IsStudentMode
            ? SelectedStudent?.StudentId : SelectedEquipment?.EquipmentId;

        History.Clear();
        if (!selectedId.HasValue)
        {
            IsBusy = false;
            StatusMessage = "Select a record to view its history.";
            return;
        }

        IsBusy = true;
        StatusMessage = "Loading borrowing history...";
        try
        {
            var results = IsStudentMode
                ? await _historyReader.GetForStudentAsync(selectedId.Value)
                : await _historyReader.GetForEquipmentAsync(selectedId.Value);

            if (request != Volatile.Read(ref _requestVersion)) return;
            // Protect against a selection changing while the request was running.
            int? currentId = IsStudentMode
                ? SelectedStudent?.StudentId : SelectedEquipment?.EquipmentId;
            if (currentId != selectedId) return;
            foreach (var borrowing in results) History.Add(borrowing);
            StatusMessage = results.Count == 0
                ? "No borrowing history for the selected record."
                : $"{results.Count} borrowing record(s) found (active and returned).";
        }
        catch (Exception)
        {
            if (request == Volatile.Read(ref _requestVersion))
            {
                History.Clear();
                StatusMessage = "Unable to load history. Verify the connection, then retry.";
            }
        }
        finally
        {
            if (request == Volatile.Read(ref _requestVersion)) IsBusy = false;
        }
    }
}
