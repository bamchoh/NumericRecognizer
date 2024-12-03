using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
using System.Windows;
using WpfApp1;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            StartApplication(RegisterServices());
        }

        private IServiceCollection RegisterServices()
        {
            var services = new ServiceCollection();

            return services;
        }


        private void StartApplication(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();

            var win = new MainWindow();

            win.DataContext = new MainWindowVM();

            win.Show();
        }
    }

}
