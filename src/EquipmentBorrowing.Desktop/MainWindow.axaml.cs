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
    private BorrowingHistoryView? _studentHistoryView;
    private BorrowingHistoryView? _equipmentHistoryView;
    private BorrowingSearchView? _borrowingSearchView;

    public MainWindow()
    {
        InitializeComponent();
    }

    public void ConfigureViews(
        EquipmentView equipmentView,
        BorrowView borrowView,
        ActiveBorrowingsView activeBorrowingsView,
        StudentManagementView studentManagementView,
        BorrowingHistoryView studentHistoryView,
        BorrowingHistoryView equipmentHistoryView,
        BorrowingSearchView borrowingSearchView)
    {
        _equipmentView = equipmentView;
        _borrowView = borrowView;
        _activeBorrowingsView = activeBorrowingsView;
        _studentManagementView = studentManagementView;
        _studentHistoryView = studentHistoryView;
        _equipmentHistoryView = equipmentHistoryView;
        _borrowingSearchView = borrowingSearchView;

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
    private void StudentHistoryButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_studentHistoryView is not null) ContentArea.Content = _studentHistoryView;
    }

    private void EquipmentHistoryButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_equipmentHistoryView is not null) ContentArea.Content = _equipmentHistoryView;
    }
    private void SearchBorrowingsButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_borrowingSearchView is not null)
            ContentArea.Content = _borrowingSearchView;
    }
}
