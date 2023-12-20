using CommunityToolkit.Mvvm.DependencyInjection;
using DataPacketLibrary.Models;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    public sealed partial class EventDetailsControl : UserControl
    {
        public EventViewModel ViewModel => (EventViewModel)DataContext;

        public event EventHandler<RoutedEventArgs> CancelButtonClick;
        public event EventHandler<RoutedEventArgs> SubmitButtonClick;

        public EventDetailsControl()
        {
            this.InitializeComponent();

            DataContext = Ioc.Default.GetRequiredService<EventViewModel>();
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

        private void UrlDeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is string blockedUrl)
            {
                ViewModel.BlockedUrls.Remove(blockedUrl);
            }
        }
    }
}
