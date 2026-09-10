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

	public ObservableCollection<Equipment> Equipment { get; } = new();

	public EquipmentViewModel(
		IEquipmentRepository equipmentRepository)
	{
		_equipmentRepository = equipmentRepository;
	}

	[RelayCommand]
	private async Task LoadEquipmentAsync()
	{
		IReadOnlyList<Equipment> equipment =
			await _equipmentRepository.GetAllAsync();

		Equipment.Clear();

		foreach (Equipment item in equipment)
		{
			Equipment.Add(item);
		}
	}
}