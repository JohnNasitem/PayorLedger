//***********************************************************************************
//Program: App.cs
//Description: Application
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Services.Actions;
using PayorLedger.Services.Database;
using PayorLedger.Services.Logger;
using PayorLedger.ViewModels;
using System.Windows;

namespace PayorLedger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Service provider for the application
        /// </summary>
        public static IServiceProvider ServiceProvider { get; private set; } = null!;



        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Init service provider
            ServiceCollection services = new();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.GetRequiredService<IDatabaseService>().InitializeTables();

            ILogger logger = ServiceProvider.GetRequiredService<ILogger>();


            DispatcherUnhandledException += (s, e) =>
            {
                logger.AddLog("UI Exception: " + e.Exception.ToString(), Logger.LogType.Error);
                MessageBox.Show("Error occured! Send the log file to John then restart the application.");
                // Prevent immediate crash
                e.Handled = true;
            };

            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                Exception? ex = e.ExceptionObject as Exception;

                if (ex != null)
                {
                    logger.AddLog("Non-UI Exception: " + ex.ToString(), Logger.LogType.Error);
                }
                else
                {
                    logger.AddLog("Non-UI Exception (unknown type): " + e.ExceptionObject, Logger.LogType.Error);
                }

                Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show("Error occured! Send the log file to John then restart the application.");
                });
            };

            TaskScheduler.UnobservedTaskException += (sender, e) =>
            {
                logger.AddLog("Unobserved Task Exception: " + e.Exception.ToString(), Logger.LogType.Error);

                MessageBox.Show("Error occured! Send the log file to John then restart the application.");
                // Prevents the exception from crashing the app in .NET Framework
                e.SetObserved();
            };
        }



        /// <summary>
        /// Configure services for the application
        /// </summary>
        /// <param name="services">service collection</param>
        private static void ConfigureServices(IServiceCollection services)
        {
            // View models
            services.AddSingleton<MainPageViewModel>();
            services.AddSingleton<PayorWindowViewModel>();
            services.AddTransient<ViewPayorViewModel>();
            services.AddSingleton<ManagePayorsViewModel>();
            services.AddSingleton<ColumnsWindowViewModel>();
            services.AddSingleton<LabelsWindowViewModel>();

            // Services
            services.AddSingleton<IUndoRedoService, UndoRedoService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<ILogger, Logger>();
        }
    }
}
