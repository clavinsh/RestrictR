using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace RestrictR
{
    public sealed partial class AppBlockingControl : UserControl
    {
        List<ApplicationInfo> Apps = new();

        ObservableCollection<ApplicationInfo> AppsFiltered;

        public AppBlockingControl()
        {
            this.InitializeComponent();

            LoadApps();
        }

        // Fills the list of all apps and the collection that will hold the filtered data
        // The collection is also set as the ListView's ItemSource (viewable UI elem)
        private void LoadApps()
        {
            Apps = ApplicationRetriever.GetInstalledApplicationsFromRegistry();
            AppsFiltered = new ObservableCollection<ApplicationInfo>(Apps);
            FilteredListView.ItemsSource = AppsFiltered;
        }

        // Event method that gets called every time the filtering input field gets changed
        // (something gets written). Updates the Collection 'AppsFiltered'
        // by querying 'Apps' - DisplayName based on the input 
        private void OnFilterChanged(object sender, TextChangedEventArgs args)
        {
            List<ApplicationInfo> TempFiltered = Apps.Where(app => app.DisplayName.Contains(FilterByFirstName.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();

            // remove all apps from observ. collection 'AppsFiltered'
            // that are in not in the newly filtered list
            for (int i = AppsFiltered.Count - 1; i >= 0; i--)
            {
                var item = AppsFiltered[i];
                if (!TempFiltered.Contains(item))
                {
                    AppsFiltered.Remove(item);
                }
            }

            // add all apps from the newly filtered list
            // to observ. collection that were missing
            foreach (var item in TempFiltered)
            {
                if (!AppsFiltered.Contains(item))
                {
                    AppsFiltered.Add(item);
                }
            }
        }
    }
}
