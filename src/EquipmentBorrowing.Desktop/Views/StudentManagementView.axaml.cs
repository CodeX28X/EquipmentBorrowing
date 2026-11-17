
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace EquipmentBorrowing.Desktop.Views;

public partial class StudentManagementView : UserControl
{
    public StudentManagementView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
