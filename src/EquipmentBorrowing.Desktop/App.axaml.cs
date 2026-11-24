using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using System;
using System.Net.Http;
using EquipmentBorrowing.Desktop.Services;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            string? configuredUrl = Environment.GetEnvironmentVariable("EQUIPMENTBORROWING_API_URL");
            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(string.IsNullOrWhiteSpace(configuredUrl)
                    ? "http://localhost:5080/api/v1/"
                    : configuredUrl.Trim().TrimEnd('/') + "/"),
                Timeout = TimeSpan.FromSeconds(15)
            };
            desktop.Exit += (_, _) => httpClient.Dispose();
            var apiClient = new HttpEquipmentBorrowingClient(httpClient);
            var studentReader = apiClient;
            var equipmentReader = apiClient;
            var borrowingReader = apiClient;
            var historyReader = apiClient;
            var searchReader = apiClient;
            var atomicBorrowingService = apiClient;
            var equipmentManagementService = apiClient;

            // Equipment catalog.
            var equipmentViewModel =
                new EquipmentViewModel(
                    equipmentReader,
                    equipmentManagementService);





            var equipmentView = new EquipmentView
            {
                DataContext = equipmentViewModel
            };

            // Borrow equipment.
            var borrowViewModel =
                new BorrowViewModel(
                    studentReader,
                    equipmentReader,
                    borrowingReader,
                    atomicBorrowingService);

            var borrowView = new BorrowView
            {
                DataContext = borrowViewModel
            };

            // Active borrowings and returns.
            var activeBorrowingsViewModel =
                new ActiveBorrowingsViewModel(
                    borrowingReader,
                    atomicBorrowingService);

            var activeBorrowingsView =
                new ActiveBorrowingsView
                {
                    DataContext = activeBorrowingsViewModel
                };

            // Student Management.

            var studentManagementService =
                 apiClient;

            var studentManagementViewModel =
                new StudentManagementViewModel(
                    studentReader,
                    studentManagementService);

            var studentManagementView =
                new StudentManagementView
                {
                    DataContext = studentManagementViewModel
                };

            // UC-07/UC-08: independent read-only history views.
            var studentHistoryView = new BorrowingHistoryView
            {
                DataContext = new BorrowingHistoryViewModel(
                    BorrowingHistoryMode.Student,
                    historyReader, studentReader, equipmentReader)
            };

            var equipmentHistoryView = new BorrowingHistoryView
            {
                DataContext = new BorrowingHistoryViewModel(
                    BorrowingHistoryMode.Equipment,
                    historyReader, studentReader, equipmentReader)
            };

            // UC-09: server-side borrowing search through HTTP.
            var borrowingSearchView = new BorrowingSearchView
            {
                DataContext = new BorrowingSearchViewModel(searchReader)
            };

            // Main window.
            var mainWindow = new MainWindow();


            mainWindow.ConfigureViews(
                equipmentView,
                borrowView,
                activeBorrowingsView,
                studentManagementView,
                studentHistoryView,
                equipmentHistoryView,
                borrowingSearchView);


            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
