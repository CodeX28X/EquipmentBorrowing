using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using EquipmentBorrowing.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;

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
            const string connectionVariable =
                "EQUIPMENTBORROWING_CONNECTION_STRING";

            string? connectionString =
                Environment.GetEnvironmentVariable(
                    connectionVariable);

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    $"Missing {connectionVariable}. " +
                    "Configure the PostgreSQL connection string.");
            }

            var options =
                new DbContextOptionsBuilder<
                    EquipmentBorrowingDbContext>()
                    .UseNpgsql(connectionString)
                    .Options;

            var contextFactory =
                new EquipmentBorrowingContextFactory(options);

            // Independent read-only repositories.
            var studentReader =
                new EfStudentReadRepository(contextFactory);

            var equipmentReader =
                new EfEquipmentReadRepository(contextFactory);

            var borrowingReader =
                new EfActiveBorrowingsReader(contextFactory);


            // UC-07/UC-08: Student and equipment borrowing histories.
            var historyReader =
                new EfBorrowingHistoryReader(contextFactory);

            // UC-09: Search, filter, and sort borrowings.
            var searchReader =
                new EfBorrowingSearchReader(contextFactory);

            var atomicBorrowingService =
                new PostgreSqlAtomicBorrowingService(contextFactory);

            var equipmentManagementService =
                new PostgreSqlEquipmentManagementService(contextFactory);

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
                 new PostgreSqlStudentManagementService(contextFactory);

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

            // UC-09: read-only PostgreSQL borrowing search.
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
