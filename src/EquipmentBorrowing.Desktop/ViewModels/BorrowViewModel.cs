using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class BorrowViewModel : ObservableObject
{
    private readonly IStudentRepository _studentRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly BorrowEquipmentService _borrowEquipmentService;

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

    public BorrowViewModel(
        IStudentRepository studentRepository,
        IEquipmentRepository equipmentRepository,
        BorrowEquipmentService borrowEquipmentService)
    {
        _studentRepository = studentRepository;
        _equipmentRepository = equipmentRepository;
        _borrowEquipmentService = borrowEquipmentService;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        IReadOnlyList<Student> students =
            await _studentRepository.GetAllAsync();

        IReadOnlyList<Equipment> equipment =
            await _equipmentRepository.GetAllAsync();

        Students.Clear();

        foreach (Student student in students)
        {
            Students.Add(student);
        }

        Equipment.Clear();

        foreach (Equipment item in equipment)
        {
            Equipment.Add(item);
        }
    }


    [RelayCommand]
    private async Task BorrowEquipmentAsync()
    {
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
            StatusMessage = "Please select an expected return date.";
            return;
        }

        BorrowResult result =
            await _borrowEquipmentService.BorrowAsync(
                SelectedStudent.StudentId,
                SelectedEquipment.EquipmentId,
                DateTime.Now.Date,
                ExpectedReturnDate.Value.Date);

        StatusMessage = result.Message;

        if (result.IsSuccess)
        {
            await LoadAsync();

            SelectedStudent = null;
            SelectedEquipment = null;
            ExpectedReturnDate = null;
        }
    }
}