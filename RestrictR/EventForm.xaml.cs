using CommunityToolkit.Mvvm.DependencyInjection;
using DataPacketLibrary.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;

namespace RestrictR
{
    // This page renders a form for event creation
    // Documentation function ID - EVENT_CREATE
    public sealed partial class EventForm : Page
    {

        private EventController _controller;

        public EventForm()
        {
            this.InitializeComponent();

            _controller = Ioc.Default.GetRequiredService<EventController>();
            eventDetailsControl.SubmitButtonClick += SubmitButtonClick;
            eventDetailsControl.CancelButtonClick += CancelButtonClick;
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            if (MainWindow.MainFrame.CanGoBack)
            {
                MainWindow.MainFrame.GoBack();
            }
        }

        private async void SubmitButtonClick(object sender, RoutedEventArgs e)
        {
            eventDetailsControl.ViewModel.ValidateAll();

            // fallback if for some reason errors are not shown after validating all
            // this can happen if some error was set but was not displayed and if it does not change
            // then the event won't be invoked - this fixes that situation by setting them either way
            eventDetailsControl.ViewModel.SetValidationErrors();

            if (!eventDetailsControl.ViewModel.HasErrors)
            {
                Event submission = ConvertToEvent(eventDetailsControl.ViewModel);

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
#nullable enable

            // when blocking all sites, the list must be empty
            // otherwise check if viewmodel list has elements and set those,
            // if not - do not create the containing object at all
            BlockedWebsites? blockedWebsites = null;
            if (viewModel.BlockAllSites)
            {
                blockedWebsites = new()
                {
                    BlockAllSites = true
                };

            }
            else if (viewModel.BlockedUrls.Count > 0)
            {
                var blockedWebsiteUrls = viewModel.BlockedUrls
                    .Select(url => new BlockedWebsiteUrl { Url = url })
                    .ToList();

                blockedWebsites = new()
                {
                    BlockAllSites = false,
                    BlockedWebsiteUrls = blockedWebsiteUrls
                };
            }

            Event converted = new()
            {
                Title = viewModel.Title,
                Start = viewModel.StartDate.Date + viewModel.StartTime,
                Duration = viewModel.Duration,
                Recurrence = viewModel.RecurrenceType,
                BlockedApps = viewModel.BlockedApplications.ToList(),
                BlockedSites = blockedWebsites
            };

            return converted;
        }
    }
}
