using CommunityToolkit.Mvvm.DependencyInjection;
using DataPacketLibrary.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.ObjectModel;
using System.Linq;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class EventEditForm : Page
    {
        private EventController _controller;

        public EventEditForm()
        {
            this.InitializeComponent();

            _controller = Ioc.Default.GetRequiredService<EventController>();
            eventDetailsControl.SubmitButtonClick += SubmitButtonClick;
            eventDetailsControl.CancelButtonClick += CancelButtonClick;
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is int eventId)
            {
                var eventToEdit = await _controller.GetEvent(eventId);
                EventViewModel viewModel = ConvertFromEvent(eventToEdit);

                eventDetailsControl.InitializeForEdit(viewModel);
            }
        }

        private static EventViewModel ConvertFromEvent(Event eventToEdit)
        {
            var evm = new EventViewModel()
            {
                EventId = eventToEdit.EventId,
                Title = eventToEdit.Title,
                StartDate = eventToEdit.Start.Date,
                StartTime = eventToEdit.Start.TimeOfDay,
                Duration = eventToEdit.Duration,
                RecurrenceType = eventToEdit.Recurrence,
                BlockedApplications = new ObservableCollection<ApplicationInfo>(eventToEdit.BlockedApps),
                BlockAllSites = eventToEdit.BlockedSites?.BlockAllSites ?? false,
                //BlockedUrls = new ObservableCollection<string>(
                //    eventToEdit.BlockedSites?.BlockedWebsiteUrls.Select(url => url.Url)
                //    ) 
                //?? new ObservableCollection<string>(),
                BlockedUrls = new ObservableCollection<string>(eventToEdit.BlockedSites?.BlockedWebsiteUrls?.Select(url => url.Url) ?? Enumerable.Empty<string>())

            };

            return evm;
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

                var result = await _controller.EditEvent(submission);

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
                EventId = viewModel.EventId,
                Title = viewModel.Title,
                Start = viewModel.StartDate.Date + viewModel.StartTime,
                Duration = viewModel.Duration,
                Recurrence = viewModel.RecurrenceType,
                BlockedApps = viewModel.BlockedApplications.ToList(),
                BlockedSites = blockedWebsites
            };

            return converted;
        }

        //public EventForm(Event eventToEdit)
        //{
        //    this.InitializeComponent();
        //    _controller = Ioc.Default.GetRequiredService<EventController>();
        //    eventDetailsControl.SubmitButtonClick += SubmitButtonClick;
        //    eventDetailsControl.CancelButtonClick += CancelButtonClick;

        //    eventDetailsControl.ViewModel.LoadEvent(eventToEdit);
        //}
    }
}
