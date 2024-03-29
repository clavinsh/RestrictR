using CommunityToolkit.Mvvm.DependencyInjection;
using DataPacketLibrary.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestrictR
{
    // user control that contains the vent form,
    // which is common between event creation and editing
    // Documentation function IDs - EVENT_EDIT, EVENT_CREATE
    public sealed partial class EventDetailsControl : UserControl
    {
        public EventViewModel ViewModel { get; set; }

        public event EventHandler<RoutedEventArgs> CancelButtonClick;
        public event EventHandler<RoutedEventArgs> SubmitButtonClick;

        public EventDetailsControl()
        {
            this.InitializeComponent();

            DataContext = Ioc.Default.GetRequiredService<EventViewModel>();
            ViewModel = (EventViewModel)DataContext;
        }

        public void InitializeForEdit(EventViewModel eventToEdit)
        {
            ViewModel = eventToEdit;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            CancelButtonClick?.Invoke(this, e);
        }

        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            SubmitButtonClick?.Invoke(this, e);
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
                        ViewModel.ValidateSomeProperty(ViewModel.IsBlockingValid, nameof(ViewModel.IsBlockingValid));
                    }
                }
            }
        }

        private void UnblockAppButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is ApplicationInfo appToUnblock)
            {
                ViewModel.BlockedApplications.Remove(appToUnblock);
                ViewModel.ValidateSomeProperty(ViewModel.IsBlockingValid, nameof(ViewModel.IsBlockingValid));
            }
        }

        private void UrlDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string blockedUrl)
            {
                ViewModel.BlockedUrls.Remove(blockedUrl);
                ViewModel.ValidateSomeProperty(ViewModel.IsBlockingValid, nameof(ViewModel.IsBlockingValid));
            }
        }
    }
}
