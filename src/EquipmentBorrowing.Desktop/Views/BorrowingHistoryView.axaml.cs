using Avalonia.Controls;
using EquipmentBorrowing.Desktop.ViewModels;

namespace EquipmentBorrowing.Desktop.Views;

public partial class BorrowingHistoryView : UserControl
{
    public BorrowingHistoryView()
    {
        InitializeComponent();
        AttachedToVisualTree += async (_, _) =>
        {
            if (DataContext is BorrowingHistoryViewModel vm)
                await vm.LoadCommand.ExecuteAsync(null);
        };
    }
}
