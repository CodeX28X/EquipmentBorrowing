
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

            // Transaction-safe write service.
            var atomicBorrowingService =
                new PostgreSqlAtomicBorrowingService(contextFactory);

            // Equipment catalog.
            var equipmentViewModel =
                new EquipmentViewModel(equipmentReader);

            var equipmentView = new EquipmentView
            {
                DataContext = equipmentViewModel
            };

            // Borrow equipment.
            var borrowViewModel =
                new BorrowViewModel(
                    studentReader,
                    equipmentReader,
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

            // Main window.
            var mainWindow = new MainWindow();

            mainWindow.ConfigureViews(
                equipmentView,
                borrowView,
                activeBorrowingsView);

            desktop.MainWindow = mainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
}
