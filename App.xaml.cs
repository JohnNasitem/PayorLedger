//***********************************************************************************
//Program: App.cs
//Description: Application
//Date: May 3, 2025
//Author: John Nasitem
//***********************************************************************************



using Microsoft.Extensions.DependencyInjection;
using PayorLedger.Services.Actions;
using PayorLedger.ViewModels;
using System.Windows;
using PayorLedger.Services.Database;
using PayorLedger.Windows.Columns;

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

            // Services
            services.AddSingleton<IUndoRedoService, UndoRedoService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
        }
    }
}
