using Avalonia.Controls;
using EquipmentBorrowing.Desktop.ViewModels;

namespace EquipmentBorrowing.Desktop.Views;

public partial class BorrowingSearchView : UserControl
{
    public BorrowingSearchView()
    {
        InitializeComponent();
        AttachedToVisualTree += async (_, _) =>
        {
            if (DataContext is BorrowingSearchViewModel vm)
                await vm.LoadCommand.ExecuteAsync(null);
        };
    }
}
