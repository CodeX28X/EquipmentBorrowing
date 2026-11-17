
using Avalonia.Controls;
using Avalonia.Interactivity;
using EquipmentBorrowing.Desktop.Views;

namespace EquipmentBorrowing.Desktop;

public partial class MainWindow : Window
{
    private EquipmentView? _equipmentView;
    private BorrowView? _borrowView;
    private ActiveBorrowingsView? _activeBorrowingsView;
    private StudentManagementView? _studentManagementView;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void ConfigureViews(
        EquipmentView equipmentView,
        BorrowView borrowView,
        ActiveBorrowingsView activeBorrowingsView,
        StudentManagementView studentManagementView)
    {
        _equipmentView = equipmentView;
        _borrowView = borrowView;
        _activeBorrowingsView = activeBorrowingsView;
        _studentManagementView = studentManagementView;

        // Default screen.
        ContentArea.Content = _equipmentView;
    }

    private void EquipmentButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_equipmentView is not null)
        {
            ContentArea.Content = _equipmentView;
        }
    }

    private void BorrowButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_borrowView is not null)
        {
            ContentArea.Content = _borrowView;
        }
    }

    private void ActiveBorrowingsButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_activeBorrowingsView is not null)
        {
            ContentArea.Content = _activeBorrowingsView;
        }
    }

    private void StudentsButton_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (_studentManagementView is not null)
        {
            ContentArea.Content = _studentManagementView;
        }
    }
}
