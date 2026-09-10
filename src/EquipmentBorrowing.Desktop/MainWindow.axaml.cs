using Avalonia.Controls;
using EquipmentBorrowing.Desktop.Views;

namespace EquipmentBorrowing.Desktop;

public partial class MainWindow : Window
{
    private EquipmentView? _equipmentView;
    private BorrowView? _borrowView;
    private ActiveBorrowingsView? _activeBorrowingsView;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void ConfigureViews(
        EquipmentView equipmentView,
        BorrowView borrowView,
        ActiveBorrowingsView activeBorrowingsView)
    {
        _equipmentView = equipmentView;
        _borrowView = borrowView;
        _activeBorrowingsView = activeBorrowingsView;

        ContentArea.Content = _equipmentView;
    }

    private void EquipmentButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_equipmentView is not null)
        {
            ContentArea.Content = _equipmentView;
        }
    }

    private void BorrowButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_borrowView is not null)
        {
            ContentArea.Content = _borrowView;
        }
    }

    private void ActiveBorrowingsButton_Click(
        object? sender,
        Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_activeBorrowingsView is not null)
        {
            ContentArea.Content = _activeBorrowingsView;
        }
    }
}