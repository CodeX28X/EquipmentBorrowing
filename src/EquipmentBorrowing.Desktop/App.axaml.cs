using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using EquipmentBorrowing.Desktop.ViewModels;
using EquipmentBorrowing.Desktop.Views;
using EquipmentBorrowing.Infrastructure.Repositories;

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
            var equipmentRepository =
                new InMemoryEquipmentRepository(
                    new[]
                    {
                        new EquipmentBorrowing.Domain.Entities.Equipment(
                            1,
                            "Laptop"),

                        new EquipmentBorrowing.Domain.Entities.Equipment(
                            2,
                            "Projector"),

                        new EquipmentBorrowing.Domain.Entities.Equipment(
                            3,
                            "Camera")
                    });

            var equipmentViewModel =
                new EquipmentViewModel(
                    equipmentRepository);

            var equipmentView =
                new EquipmentView
                {
                    DataContext = equipmentViewModel
                };

            desktop.MainWindow =
                new MainWindow
                {
                    Content = equipmentView
                };
        }

        base.OnFrameworkInitializationCompleted();
    }
}