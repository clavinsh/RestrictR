using CommunityToolkit.Mvvm.DependencyInjection;
using DataPacketLibrary.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;

namespace RestrictR
{
    // This page renders a list of all events in the database for administrators
    // Documentation function IDs - EVENT_VIEW_LIST, EVENT_VIEW
    public sealed partial class EventList : Page
    {

        public ObservableCollection<Event> Events;

        private EventController _controller;

        public EventList()
        {
            this.InitializeComponent();
            _controller = Ioc.Default.GetRequiredService<EventController>();

            Events = new ObservableCollection<Event>();

            Loaded += EventList_Loaded;
        }

        private async void EventList_Loaded(object sender, RoutedEventArgs e)
        {
            var eventsList = await _controller.GetEvents();

            foreach (var ev in eventsList)
            {
                Events.Add(ev);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.MainFrame.Navigate(typeof(EventForm));
        }

        public string FormatStartDuration(Event someEvent)
        {
            return $"{someEvent.Start} - {someEvent.Start.Add(someEvent.Duration)}";
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Event eventItem)
            {

                MainWindow.MainFrame.Navigate(typeof(EventEditForm), eventItem.EventId);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is Event eventItem)
            {
                var confirmDialog = new ContentDialog()
                {
                    XamlRoot = this.XamlRoot,
                    Title = "Confirm Deletion",
                    Content = "Are you sure you want to delete this event?",
                    PrimaryButtonText = "Delete",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close
                };

                var result = await confirmDialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    var controllerResult = await _controller.DeleteEvent(eventItem);

                    if (controllerResult.Success)
                    {
                        MainWindow.MainFrame.Navigate(typeof(EventList));
                    }
                    else
                    {
                        ContentDialog dialog = new()
                        {
                            XamlRoot = this.XamlRoot,
                            Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                            Title = "Ran into an error while trying to delete the blocking event",
                            CloseButtonText = "OK",
                            DefaultButton = ContentDialogButton.Close,
                            Content = controllerResult.Error
                        };
                        await dialog.ShowAsync();
                    }
                }
            }
        }

    }
}
