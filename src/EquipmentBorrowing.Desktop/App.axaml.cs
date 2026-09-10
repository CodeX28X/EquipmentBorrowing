using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Application.Services;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Repositories;
using EquipmentBorrowing.Domain.Entities;

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
            // ================================
            // SHARED REPOSITORIES
            // ================================

            var studentRepository =
                new InMemoryStudentRepository(
                    new[]
                    {
                        new Student(1, "Juan Dela Cruz", 3, true),
                        new Student(2, "Maria Santos", 2, true),
                        new Student(3, "Pedro Reyes", 1, false)
                    });

            var equipmentRepository =
                new InMemoryEquipmentRepository(
                    new[]
                    {
                        new Equipment(1, "Laptop"),
                        new Equipment(2, "Projector"),
                        new Equipment(3, "Camera")
                    });

            var borrowingRepository =
                new InMemoryBorrowingRepository();

            // ================================
            // APPLICATION SERVICE
            // ================================

            var borrowEquipmentService =
                new BorrowEquipmentService(
                    studentRepository,
                    equipmentRepository,
                    borrowingRepository,
                    3);

            // ================================
            // BORROW VIEW MODEL
            // ================================

            var borrowViewModel =
                new BorrowViewModel(
                    studentRepository,
                    equipmentRepository,
                    borrowEquipmentService);

            // ================================
            // BORROW VIEW
            // ================================

            var borrowView =
                new BorrowView
                {
                    DataContext = borrowViewModel
                };

            // ================================
            // SHOW BORROW VIEW
            // ================================

            desktop.MainWindow =
                new MainWindow
                {
                    Content = borrowView
                };
        }

        base.OnFrameworkInitializationCompleted();
    }
}