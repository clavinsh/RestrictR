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
        List<ApplicationInfo> Apps = new();

        ObservableCollection<ApplicationInfo> AppsFiltered;

        public static Frame MainFrame { get; private set; }

        public readonly bool Admin;

        public MainWindow()
        {
            this.InitializeComponent();

            MainFrame = mainFrame;
            mainFrame.Navigate(typeof(EventList));

            Admin = IsUserAdmin();
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


        //private void OnFormValidityChanged(object recipient, FormValidityChangedMessage message)
        //{
        //    eventFormContentDialog.IsPrimaryButtonEnabled = message.IsFormValid;
        //}

        //// Unregister the message when the ContentDialog is closed
        //private void OnContentDialogClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
        //{
        //    _messenger.Unregister<FormValidityChangedMessage>(this);
        //}

        // Fills the list of all apps and the collection that will hold the filtered data
        // The collection is also set as the ListView's ItemSource (viewable UI elem)
        //private void LoadApps()
        //{
        //    Apps = ApplicationRetriever.GetInstalledApplicationsFromRegistry();
        //    AppsFiltered = new ObservableCollection<ApplicationInfo>(Apps);
        //    FilteredListView.ItemsSource = AppsFiltered;
        //    ImageInfoBar.Message = Apps.Count.ToString();
        //    ImageInfoBar.IsOpen = true;
        //}

        //private async void AppBarButton_Click(object sender, RoutedEventArgs e)
        //{
        //    //FolderPicker folderPicker = new FolderPicker();
        //    //folderPicker.FileTypeFilter.Add("*");

        //    //var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

        //    //WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hwnd);

        //    //var folder = await folderPicker.PickSingleFolderAsync();

        //    //if (folder != null)
        //    //{
        //    //    LoadImages(folder.Path);
        //    //}
        //}

        //private async void Button_Click(object sender, RoutedEventArgs e)
        //{
        //    if (sender is Button b)
        //    {
        //        List<ApplicationInfo> appsToBlock = new();
        //        var selectedApps = FilteredListView.SelectedItems.ToList();

        //        foreach (var app in selectedApps)
        //        {
        //            appsToBlock.Add((ApplicationInfo)app);
        //        }

        //        await blockingConfig.SetBlockedApps(appsToBlock);
        //    }
        //}

        //private static void SetWindowSize(Window window, int height, int width)
        //{
        //    var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
        //    var windowsId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hwnd);
        //    var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowsId);
        //    appWindow.Resize(new Windows.Graphics.SizeInt32(height, width));
        //}

        //SpringVector3NaturalMotionAnimation _springAnimation;

        //private void CreateOrUpdateSpringAnimation(float finalValue)
        //{
        //    if (_springAnimation == null)
        //    {
        //        _springAnimation = this.Compositor.CreateSpringVector3Animation();
        //        _springAnimation.Target = "Scale";
        //    }

        //    _springAnimation.FinalValue = new Vector3(finalValue);
        //}

        //private void element_PointerEntered(object sender, PointerRoutedEventArgs e)
        //{
        //    CreateOrUpdateSpringAnimation(1.15f);

        //    (sender as UIElement).StartAnimation(_springAnimation);
        //}

        //private void element_PointerExited(object sender, PointerRoutedEventArgs e)
        //{
        //    CreateOrUpdateSpringAnimation(1.0f);

        //    (sender as UIElement).StartAnimation(_springAnimation);
        //}

        //// Event method that gets called every time the filtering input field gets changed
        //// (something gets written). Updates the Collection 'AppsFiltered'
        //// by querying 'Apps' - DisplayName based on the input 
        //private void OnFilterChanged(object sender, TextChangedEventArgs args)
        //{
        //    List<ApplicationInfo> TempFiltered = Apps.Where(app => app.DisplayName.Contains(FilterByFirstName.Text, StringComparison.InvariantCultureIgnoreCase)).ToList();

        //    // remove all apps from observ. collection 'AppsFiltered'
        //    // that are in not in the newly filtered list
        //    for (int i = AppsFiltered.Count - 1; i >= 0; i--)
        //    {
        //        var item = AppsFiltered[i];
        //        if (!TempFiltered.Contains(item))
        //        {
        //            AppsFiltered.Remove(item);
        //        }
        //    }

        //    // add all apps from the newly filtered list
        //    // to observ. collection that were missing
        //    foreach (var item in TempFiltered)
        //    {
        //        if (!AppsFiltered.Contains(item))
        //        {
        //            AppsFiltered.Add(item);
        //        }
        //    }
        //}

        //private async void OpenNewEventFormButton_Click(object sender, RoutedEventArgs e)
        //{
        //    eventFormContentDialog.Content = new EventForm(_messenger);

        //    //EventForm form = new EventForm();

        //    //ContentDialog dialog = new()
        //    //{
        //    //    // XamlRoot must be set in the case of a ContentDialog running in a Desktop app
        //    //    XamlRoot = this.Content.XamlRoot,
        //    //    Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
        //    //    Title = "Save your work?",
        //    //    PrimaryButtonText = "Save",
        //    //    CloseButtonText = "Cancel",
        //    //    DefaultButton = ContentDialogButton.Primary,
        //    //    Content = form
        //    //};

        //    //BindingOperations.SetBinding(dialog, ContentDialog.IsPrimaryButtonEnabledProperty, binding);

        //    var result = await eventFormContentDialog.ShowAsync();
        //}

        //private async void Button_Click_2(object sender, RoutedEventArgs e)
        //{
        //    if(sender is Button b)
        //    {
        //        await blockingConfig.SetUnblockALLSites();
        //    }
        //}
    }
}
