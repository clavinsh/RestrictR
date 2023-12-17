using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.UI;
using DataPacketLibrary;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Media3D;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using DataPacketLibrary.Models;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EventForm : Page
    {
        public EventViewModel ViewModel => (EventViewModel)DataContext;

        //private BlockingConfigurator blockingConfigurator;
        private EventController _controller;


        public EventForm()
        {
            this.InitializeComponent();
            DataContext = Ioc.Default.GetRequiredService<EventViewModel>();

            _controller = Ioc.Default.GetRequiredService<EventController>();
        }

        // submits the new event to the service
        private async void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // the viewmodel is converted to an event
            if (!ViewModel.HasErrors)
            {
                Event submission = ConvertToEvent(ViewModel);

                var result = await _controller.CreateEvent(submission);

                if (result.Success)
                {
                    MainWindow.MainFrame.Navigate(typeof(EventList));
                }
                else
                {
                    ContentDialog dialog = new()
                    {
                        XamlRoot = this.XamlRoot,
                        Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                        Title = "Ran into an error while trying to save the blocking event",
                        CloseButtonText = "OK",
                        DefaultButton = ContentDialogButton.Close,
                        Content = result.Error
                    };
                    await dialog.ShowAsync();
                }
            }
        }

        private static Event ConvertToEvent(EventViewModel viewModel)
        {
            Event converted = new()
            {
                Start = viewModel.StartDate.Date + viewModel.StartTime,
                Duration = viewModel.Duration,
                Recurrence = viewModel.RecurrenceType,
                BlockedApps = viewModel.BlockedApplications.ToList()
            };

            return converted;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.MainFrame.CanGoBack)
            {
                MainWindow.MainFrame.GoBack();
            }
        }

        // Event method that gets called every time the filtering input field gets changed
        // (something gets written). Updates the Collection 'AppsFiltered'
        // by querying 'Apps' - DisplayName based on the input 
        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            List<ApplicationInfo> TempFiltered = ViewModel.Apps.Where(app => app.DisplayName.Contains(FilterByFirstName.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();

            // remove all apps from observ. collection 'AppsFiltered'
            // that are in not in the newly filtered list
            for (int i = ViewModel.AppsFiltered.Count - 1; i >= 0; i--)
            {
                var item = ViewModel.AppsFiltered[i];
                if (!TempFiltered.Contains(item))
                {
                    ViewModel.AppsFiltered.Remove(item);
                }
            }

            // add all apps from the newly filtered list
            // to observ. collection that were missing
            foreach (var item in TempFiltered)
            {
                if (!ViewModel.AppsFiltered.Contains(item))
                {
                    ViewModel.AppsFiltered.Add(item);
                }
            }
        }

        private void FilteredListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView)
            {
                foreach (var item in e.AddedItems)
                {
                    if (!ViewModel.BlockedApplications.Contains(item as ApplicationInfo))
                    {
                        ViewModel.BlockedApplications.Add(item as ApplicationInfo);
                    }
                }
            }
        }

        private void UnblockAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ApplicationInfo appToUnblock)
            {
                ViewModel.BlockedApplications.Remove(appToUnblock);
            }
        }
    }
}
