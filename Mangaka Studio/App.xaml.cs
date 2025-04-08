using System.Configuration;
using System.Data;
using System.Windows;
using Mangaka_Studio.Controls;
using Mangaka_Studio.Services;
using Mangaka_Studio.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Mangaka_Studio
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly ServiceProvider serviceProvider;

        public App()
        {
            var services = new ServiceCollection();

            ConfigureServices(services);

            serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection serviceDescriptors)
        {
            serviceDescriptors.AddSingleton<IToolsFactory, ToolsFactory>();

            serviceDescriptors.AddSingleton<ToolsViewModel>();

            serviceDescriptors.AddSingleton<CanvasViewModel>();

            serviceDescriptors.AddSingleton<ColorPickerViewModel>();

            serviceDescriptors.AddSingleton<LayerViewModel>();

            serviceDescriptors.AddSingleton<MainViewModel>();

            serviceDescriptors.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetService<MainWindow>();
            if (mainWindow == null)
            {
                MessageBox.Show("Ошибка: MainWindow не создан");
                return;
            }
            mainWindow.Show();
        }
    }

}
