
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowViewModel : ObservableObject
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IAtomicBorrowingService _atomicBorrowingService;

    public ObservableCollection<Student> Students { get; } = new();

    public ObservableCollection<Equipment> Equipment { get; } = new();

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private DateTime? expectedReturnDate;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public BorrowViewModel(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        IAtomicBorrowingService atomicBorrowingService)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
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
            var students =
                await _studentRepository.GetAllAsync();

            var equipment =
                await _equipmentRepository.GetAllAsync();

            Students.Clear();

            foreach (var student in students)
                Students.Add(student);

            Equipment.Clear();

            foreach (var item in equipment)
                Equipment.Add(item);

            SelectedStudent = null;
            SelectedEquipment = null;
            ExpectedReturnDate = null;
            StatusMessage = string.Empty;
        }
        catch (Exception)
        {
            StatusMessage =
                "Unable to load student and equipment records. " +
                "Check the database connection and retry.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task BorrowEquipmentAsync()
    {
        if (IsBusy)
            return;

        if (SelectedStudent is null)
        {
            StatusMessage = "Please select a student.";
            return;
        }

        if (SelectedEquipment is null)
        {
            StatusMessage = "Please select equipment.";
            return;
        }

        if (ExpectedReturnDate is null)
        {
            StatusMessage =
                "Please select an expected return date.";
            return;
        }

        DateOnly checkoutDate = GetManilaToday();

        DateOnly returnDate =
            DateOnly.FromDateTime(ExpectedReturnDate.Value);

        if (returnDate <= checkoutDate)
        {
            StatusMessage =
                "Expected return date must be later than today.";
            return;
        }

        int studentId = SelectedStudent.StudentId;
        int equipmentId = SelectedEquipment.EquipmentId;

        IsBusy = true;

        try
        {
            var result = await _atomicBorrowingService.CheckoutAsync(
                studentId,
                new[] { equipmentId },
                checkoutDate,
                returnDate);

            StatusMessage = result.Message;

            if (result.Status ==
                BorrowingOperationStatus.Success)
            {
                // Reload after a successful database commit.
                var equipment =
                    await _equipmentRepository.GetAllAsync();

                Equipment.Clear();

                foreach (var item in equipment)
                    Equipment.Add(item);

                SelectedEquipment = null;
                ExpectedReturnDate = null;
            }
            else if (result.Status ==
                     BorrowingOperationStatus.OutcomeUnknown)
            {
                // Do not automatically retry an uncertain commit.
                StatusMessage =
                    result.Message +
                    " Do not submit again until you verify " +
                    "the current borrowing records.";
            }
        }
        catch (Exception)
        {
            StatusMessage =
                "Checkout did not complete normally. " +
                "Its outcome may be uncertain. " +
                "Verify the borrowing records before retrying.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
