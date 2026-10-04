
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Rendering.Composition;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Domain.Entities;
using EquipmentBorrowing.Infrastructure.Persistence;
using EquipmentBorrowing.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Intrinsics.X86;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EquipmentBorrowing.Desktop;

public partial class App : Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // ---------------------------------------------------------
            // DATABASE
            // ---------------------------------------------------------

            var dbOptions =
                new DbContextOptionsBuilder<EquipmentBorrowingDbContext>()
                    .UseSqlite("Data Source=EquipmentBorrowing.db")
                    .Options;

            // Initialize database and seed data.
            using (var initializationContext =
                   new EquipmentBorrowingDbContext(dbOptions))
            {
                DatabaseSeeder
                    .InitializeAsync(initializationContext)
                    .GetAwaiter()
                    .GetResult();
            }

            // ---------------------------------------------------------
            // SHARED APPLICATION DbContext
            // ---------------------------------------------------------

            var dbContext =
                new EquipmentBorrowingDbContext(dbOptions);

            // ---------------------------------------------------------
            // EF REPOSITORIES
            // ---------------------------------------------------------

            var studentRepository =
                new EfStudentRepository(dbContext);

            var equipmentRepository =
                new EfEquipmentRepository(dbContext);

            var borrowingRepository =
                new EfBorrowingRepository(dbContext);

            // ---------------------------------------------------------
            // APPLICATION SERVICES
            // ---------------------------------------------------------

            var borrowEquipmentService =
                new BorrowEquipmentService(
                    studentRepository,
                    equipmentRepository,
                    borrowingRepository,
                    3);

            var returnEquipmentService =
                new ReturnEquipmentService(
                    borrowingRepository);

            // ---------------------------------------------------------
            // EQUIPMENT VIEW
            // ---------------------------------------------------------

            var equipmentViewModel =
                new EquipmentViewModel(
                    equipmentRepository);

            var equipmentView =
                new EquipmentView
                {
                    DataContext = equipmentViewModel
                };

            // ---------------------------------------------------------
            // BORROW VIEW
            // ---------------------------------------------------------

            var borrowViewModel =
                new BorrowViewModel(
                    studentRepository,
                    equipmentRepository,
                    borrowEquipmentService);

            var borrowView =
                new BorrowView
                {
                    DataContext = borrowViewModel
                };

            // ---------------------------------------------------------
            // ACTIVE BORROWINGS VIEW
            // ---------------------------------------------------------

            var activeBorrowingsViewModel =
                new ActiveBorrowingsViewModel(
                    borrowingRepository,
                    returnEquipmentService);

            var activeBorrowingsView =
                new ActiveBorrowingsView
                {
                    DataContext = activeBorrowingsViewModel
                };

            // ---------------------------------------------------------
            // MAIN WINDOW
            // ---------------------------------------------------------

            var mainWindow = new MainWindow();

            mainWindow.ConfigureViews(
                equipmentView,
                borrowView,
                activeBorrowingsView);

            desktop.MainWindow = mainWindow;

            // Dispose the shared DbContext when the application exits.
            desktop.ShutdownRequested += (_, _) =>
            {
                dbContext.Dispose();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}