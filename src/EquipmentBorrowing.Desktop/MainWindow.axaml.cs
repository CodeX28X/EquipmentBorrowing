using Avalonia.Controls;
using EquipmentBorrowing.Desktop.Views;

namespace EquipmentBorrowing.Desktop;

public partial class MainWindow : Window
{
    private readonly EquipmentView _equipmentView;
    private readonly BorrowView _borrowView;
    private readonly ActiveBorrowingsView _activeBorrowingsView;

    public MainWindow(
        EquipmentView equipmentView,
        BorrowView borrowView,
        ActiveBorrowingsView activeBorrowingsView)
    {
        InitializeComponent();

        _equipmentView = equipmentView;
        _borrowView = borrowView;
        _activeBorrowingsView = activeBorrowingsView;

        ContentArea.Content = _equipmentView;
    }

    private void EquipmentButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        ContentArea.Content = _equipmentView;
    }

    private void BorrowButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        ContentArea.Content = _borrowView;
    }

    private void ActiveBorrowingsButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        ContentArea.Content = _activeBorrowingsView;
    }
}