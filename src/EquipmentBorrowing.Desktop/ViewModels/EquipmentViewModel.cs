
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EquipmentBorrowing.Application;
using EquipmentBorrowing.Domain.Entities;

namespace EquipmentBorrowing.Desktop.ViewModels;

public partial class EquipmentViewModel : ObservableObject
{
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IEquipmentManagementService _managementService;

    public ObservableCollection<Equipment> Equipment { get; } = new();


    public ObservableCollection<Equipment> FilteredEquipment { get; } = new();

    [ObservableProperty]
    private bool showAvailableOnly;

    [ObservableProperty]
    private string filterStatusMessage = string.Empty;

    partial void OnShowAvailableOnlyChanged(bool value)
    {
        ApplyEquipmentFilter();
    }


    [ObservableProperty]
    private Equipment? selectedEquipment;

    [ObservableProperty]
    private string equipmentIdentifier = string.Empty;

    [ObservableProperty]
    private string equipmentName = string.Empty;

    [ObservableProperty]
    private string description = string.Empty;

    [ObservableProperty]
    private string statusMessage = string.Empty;

    [ObservableProperty]
    private bool isBusy;

    public bool IsEditing => SelectedEquipment is not null;

    public string FormTitle =>
        IsEditing ? "Edit equipment" : "Register new equipment";

    public EquipmentViewModel(
        IEquipmentRepository equipmentRepository,
        IEquipmentManagementService managementService)
    {
        _equipmentRepository = equipmentRepository
            ?? throw new ArgumentNullException(nameof(equipmentRepository));

        _managementService = managementService
            ?? throw new ArgumentNullException(nameof(managementService));
    }

    partial void OnSelectedEquipmentChanged(Equipment? value)
    {
        OnPropertyChanged(nameof(IsEditing));
        OnPropertyChanged(nameof(FormTitle));

        EquipmentIdentifier =
            value?.EquipmentIdentifier ?? string.Empty;

        EquipmentName =
            value?.EquipmentName ?? string.Empty;

        Description =
            value?.Description ?? string.Empty;
    }


    private void ApplyEquipmentFilter()
    {
        // Preserve the current editor selection only if it
        // remains visible under the selected filter.
        if (SelectedEquipment is not null &&
            ShowAvailableOnly &&
            !SelectedEquipment.IsAvailable)
        {
            SelectedEquipment = null;
        }

        FilteredEquipment.Clear();

        foreach (Equipment item in Equipment)
        {
            if (!ShowAvailableOnly || item.IsAvailable)
            {
                FilteredEquipment.Add(item);
            }
        }

        if (Equipment.Count == 0)
        {
            FilterStatusMessage = "No equipment registered yet.";
        }
        else if (ShowAvailableOnly && FilteredEquipment.Count == 0)
        {
            FilterStatusMessage = "No available equipment found.";
        }
        else if (ShowAvailableOnly)
        {
            FilterStatusMessage =
                $"{FilteredEquipment.Count} of {Equipment.Count} " +
                "equipment item(s) available.";
        }
        else
        {
            FilterStatusMessage =
                $"Showing all {Equipment.Count} equipment item(s).";
        }
    }



    private async Task RefreshEquipmentAsync(
        int? selectEquipmentId = null)
    {
        IReadOnlyList<Equipment> equipment =
            await _equipmentRepository.GetAllAsync();

        // Populate the complete equipment collection
        // using actual records retrieved from PostgreSQL.
        Equipment.Clear();

        foreach (Equipment item in equipment)
        {
            Equipment.Add(item);
        }

        SelectedEquipment = null;

        // Update the visible equipment list.
        ApplyEquipmentFilter();

        // Restore selection only if the equipment
        // is visible under the current filter.
        if (selectEquipmentId.HasValue)
        {
            foreach (Equipment item in FilteredEquipment)
            {
                if (item.EquipmentId == selectEquipmentId.Value)
                {
                    SelectedEquipment = item;
                    break;
                }
            }
        }
    }


    [RelayCommand]
    private async Task LoadEquipmentAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;

        try
        {
            await RefreshEquipmentAsync();

            StatusMessage = Equipment.Count == 0
                ? "No equipment registered yet."
                : $"{Equipment.Count} equipment item(s) loaded.";
        }
        catch (Exception)
        {
            StatusMessage =
                "Unable to load equipment. Check the database connection.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void NewEquipment()
    {
        if (IsBusy)
            return;

        SelectedEquipment = null;
        StatusMessage = "Enter the new equipment information.";
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (IsBusy)
            return;

        if (string.IsNullOrWhiteSpace(EquipmentIdentifier))
        {
            StatusMessage = "Equipment identifier is required.";
            return;
        }

        if (string.IsNullOrWhiteSpace(EquipmentName))
        {
            StatusMessage = "Equipment name is required.";
            return;
        }

        IsBusy = true;

        try
        {
            EquipmentOperationResult result;

            if (SelectedEquipment is null)
            {
                result = await _managementService.RegisterAsync(
                    EquipmentIdentifier,
                    EquipmentName,
                    Description);
            }
            else
            {
                result = await _managementService.UpdateAsync(
                    SelectedEquipment.EquipmentId,
                    EquipmentIdentifier,
                    EquipmentName,
                    Description);
            }

            StatusMessage = result.Message;

            if (result.Status == EquipmentOperationStatus.Success)
            {
                await RefreshEquipmentAsync(result.EquipmentId);
                StatusMessage = result.Message;
            }
            else if (result.Status ==
                     EquipmentOperationStatus.OutcomeUnknown)
            {
                StatusMessage =
                    "The change may have been saved. " +
                    "Refresh equipment records and verify before retrying.";
            }
        }
        catch (Exception)
        {
            StatusMessage =
                "The operation could not be confirmed. " +
                "Refresh equipment records before retrying.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
