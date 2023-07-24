using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Horizon.AppointmentKeeper.Context;
using Horizon.AppointmentKeeper.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client.Extensions.Msal;

namespace Horizon.AppointmentKeeper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider serviceProvider;
        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            serviceProvider = services.BuildServiceProvider();
        }
        private void ConfigureServices(ServiceCollection services)
        {
            //services.AddDbContext<EmployeeDbContext>(options =>
            //{
            //    options.UseSqlite("Data Source = Employee.db");
            //});
            services.AddLogging(l=>l.AddEventLog());
            services.AddSingleton<FontService>();
            services.AddSingleton<GraphService>();
            services.AddSingleton<CalendarService>();
            services.AddSingleton<SettingsContext>();
            services.AddSingleton<SoundService>();
            services.AddTransient<TimerContext>();
            services.AddTransient<About>();
            services.AddTransient<Settings>();
            services.AddSingleton<MainWindow>();

        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
