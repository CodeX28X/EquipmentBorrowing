
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class StudentManagementViewModel : ObservableObject
{
    private readonly IStudentRepository _studentRepository;
    private readonly IStudentManagementService _managementService;

    public ObservableCollection<Student> Students { get; } = new();

    [ObservableProperty]
    private Student? selectedStudent;

    [ObservableProperty]
    private string studentNumber = string.Empty;

    [ObservableProperty]
    private string studentName = string.Empty;

    [ObservableProperty]
    private string contactInformation = string.Empty;

    [ObservableProperty]
    private string studentYear = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public bool IsEditing => SelectedStudent is not null;

    public string FormTitle =>
        IsEditing ? "Edit student" : "Register new student";

    public StudentManagementViewModel(
        IStudentRepository studentRepository,
        IStudentManagementService managementService)
    {
        _studentRepository = studentRepository;
        _managementService = managementService;
    }

    partial void OnSelectedStudentChanged(Student? value)
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));

        StudentNumber = value?.StudentNumber ?? string.Empty;
        StudentName = value?.StudentName ?? string.Empty;
        ContactInformation = value?.ContactInformation ?? string.Empty;

        StudentYear = value?.StudentYear is int year
            ? year.ToString(CultureInfo.InvariantCulture)
            : string.Empty;
    }

    private bool TryGetStudentYear(out int? year)
    {
        year = null;

        if (string.IsNullOrWhiteSpace(StudentYear))
            return true;

        if (!int.TryParse(
                StudentYear.Trim(),
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int parsed) || parsed <= 0)
        {
            StatusMessage =
                "Student year must be a positive whole number.";
            return false;
        }

        year = parsed;
        return true;
    }

    private async Task RefreshStudentsAsync(int? selectStudentId = null)
    {
        var students = await _studentRepository.GetAllAsync();

        Students.Clear();

        foreach (var student in students)
            Students.Add(student);

        SelectedStudent = null;

        if (selectStudentId.HasValue)
        {
            foreach (var student in Students)
            {
                if (student.StudentId == selectStudentId.Value)
                {
                    SelectedStudent = student;
                    break;
                }
            }
        }
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await RefreshStudentsAsync();
            StatusMessage = Students.Count == 0
                ? "No students registered yet."
                : $"{Students.Count} student(s) loaded.";
        }
        catch (Exception)
        {
            StatusMessage =
                "Unable to load students. Check the database connection.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewStudent()
    {
        if (IsBusy)
            return;

        SelectedStudent = null;
        StatusMessage = "Enter the new student's information.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(StudentNumber))
        {
            StatusMessage = "Student number is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(StudentName))
        {
            StatusMessage = "Student name is required.";
            return;
        }

        if (!TryGetStudentYear(out int? year))
            return;

        IsBusy = true;

        try
        {
            StudentOperationResult result;

            if (SelectedStudent is null)
            {
                result = await _managementService.RegisterAsync(
                    StudentNumber,
                    StudentName,
                    ContactInformation,
                    year);
            }
            else
            {
                result = await _managementService.UpdateAsync(
                    SelectedStudent.StudentId,
                    StudentNumber,
                    StudentName,
                    ContactInformation,
                    year);
            }

            StatusMessage = result.Message;

            if (result.Status == StudentOperationStatus.Success)
            {
                await RefreshStudentsAsync(result.StudentId);
                StatusMessage = result.Message;
            }
            else if (result.Status ==
                     StudentOperationStatus.OutcomeUnknown)
            {
                StatusMessage =
                    "The change may have been saved. " +
                    "Refresh the student list and verify before retrying.";
            }
        }
        catch (Exception)
        {
            StatusMessage =
                "The operation could not be confirmed. " +
                "Refresh the records before trying again.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleEligibilityAsync()
    {
        if (IsBusy || SelectedStudent is null)
            return;

        int studentId = SelectedStudent.StudentId;
        bool newEligibility = !SelectedStudent.IsAllowedToBorrow;

        IsBusy = true;

        try
        {
            var result = await _managementService.SetEligibilityAsync(
                studentId,
                newEligibility);

            StatusMessage = result.Message;

            if (result.Status == StudentOperationStatus.Success)
            {
                await RefreshStudentsAsync(studentId);
                StatusMessage = result.Message;
            }
            else if (result.Status ==
                     StudentOperationStatus.OutcomeUnknown)
            {
                StatusMessage =
                    "Eligibility may have changed. " +
                    "Refresh and verify before trying again.";
            }
        }
        catch (Exception)
        {
            StatusMessage =
                "Eligibility could not be confirmed. " +
                "Refresh student records before retrying.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
