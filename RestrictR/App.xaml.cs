﻿using CommunityToolkit.Mvvm.DependencyInjection;
using DataPacketLibrary.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using RestrictR.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        //private IMessenger _messenger { get; set; } = new();

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }


        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            Ioc.Default.ConfigureServices(
                new ServiceCollection()
                .AddSingleton<IDialogService, DialogService>()
                .AddTransient<EventViewModel>()
                .AddDbContext<RestrictRDbContext>()
                .AddScoped<EventController>()
                .BuildServiceProvider());

            m_window = new MainWindow();
            m_window.Activate();
        }

        private Window m_window;
    }
}
