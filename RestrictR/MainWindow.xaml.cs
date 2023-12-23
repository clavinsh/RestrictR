using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using DataPacketLibrary.Models;
using System.Security.Principal;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static Frame MainFrame { get; private set; }

        public readonly bool Admin;

        public MainWindow()
        {
            this.InitializeComponent();
            MainFrame = mainFrame;
            Admin = IsUserAdmin();
            Title = "RestrictR";

            if (Admin)
            {
                mainFrame.Navigate(typeof(EventList));
            }
            else
            {
                mainFrame.Navigate(typeof(EventListUser));
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainFrame.CanGoForward)
            {
                MainFrame.GoForward();
            }
        }

        private void MainFrame_Navigated(object sender, Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            OnPropertyChanged(nameof(MainFrame.CanGoBack));
            OnPropertyChanged(nameof(MainFrame.CanGoForward));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static bool IsUserAdmin()
        {
            WindowsIdentity currentIdentity = WindowsIdentity.GetCurrent();
            WindowsPrincipal currentPrincipal = new WindowsPrincipal(currentIdentity);

            // Check if the current user is a member of the Administrator group
            bool isAdmin = currentPrincipal.IsInRole(WindowsBuiltInRole.Administrator);

            return isAdmin;
        }

        private void navView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                mainFrame.Navigate(typeof(Settings));
            }
            else
            if (args.InvokedItemContainer is NavigationViewItem item)
            {
                var tag = item.Tag.ToString();

                switch(tag)
                {
                    case "EventsPage":
                        if (Admin)
                        {
                            mainFrame.Navigate(typeof(EventList));
                        }
                        else
                        {
                            mainFrame.Navigate(typeof(EventListUser));
                        }
                        break;
                    case "HelpPage":
                        mainFrame.Navigate(typeof(Help));
                        break;
                }
            }
        }

        private void navView_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (MainFrame.CanGoBack)
            {
                MainFrame.GoBack();
            }
        }
    }
}
