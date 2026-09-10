using Avalonia.Controls;
using EquipmentBorrowing.Desktop.ViewModels;

namespace EquipmentBorrowing.Desktop.Views;

public partial class EquipmentView : UserControl
{
    public EquipmentView()
    {
        InitializeComponent();

        AttachedToVisualTree += async (_, _) =>
        {
            if (DataContext is EquipmentViewModel viewModel)
            {
                await viewModel.LoadEquipmentCommand.ExecuteAsync(null);
            }
        };
    }
}